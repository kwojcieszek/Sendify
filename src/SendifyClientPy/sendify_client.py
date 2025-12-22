"""
sendify_client.py

Minimal client for:
POST {host}/api/v1/messages
Content-Type: application/json
Authorization: Bearer <token>

"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Dict, List, Optional
import time
import base64
from pathlib import Path
import mimetypes

try:
    import requests
except ImportError as e:
    raise ImportError(
        "This client requires 'requests'. Install with: pip install requests"
    ) from e


JsonDict = Dict[str, Any]


@dataclass(frozen=True)
class SendifyConfig:
    host: str  # e.g. "https://sendify.radwag.pl" or "sendify.radwag.pl"
    token: str  # bearer token string (without "Bearer ")
    timeout: float = 15.0  # seconds
    retries: int = 2  # number of retry attempts on transient failures
    backoff_seconds: float = 0.6  # base backoff between retries
    verify_ssl: bool = False  # whether to verify SSL certificates


@dataclass(frozen=True)
class Attachment:
    """
    Represents a single attachment to be sent with the message.

    Content is base64-encoded binary. Use `Attachment.from_file(path, ...)`
    to create an attachment from disk.
    """
    file_name: str
    content_type: str
    content: str

    def to_api_dict(self) -> JsonDict:
        return {
            "FileName": self.file_name,
            "ContentType": self.content_type,
            "Content": self.content,
        }

    @classmethod
    def from_file(
        cls,
        path: str,
        *,
        file_name: Optional[str] = None,
        content_type: Optional[str] = None,
        encoding: str = "base64",
    ) -> "Attachment":
        """
        Create Attachment by reading `path` from disk and encoding its bytes.

        - `path`: filesystem path to the file.
        - `file_name`: optional name to send to API (defaults to the file's basename).
        - `content_type`: optional MIME type (guessed if not provided).
        - `encoding`: currently supports "base64" (default).
        """
        p = Path(path)
        if not p.exists():
            raise FileNotFoundError(f"Attachment file not found: {path}")
        if not p.is_file():
            raise IsADirectoryError(f"Attachment path is not a file: {path}")

        raw = p.read_bytes()

        if encoding.lower() != "base64":
            raise ValueError(f"Unsupported encoding: {encoding}")

        b64 = base64.b64encode(raw).decode("ascii")

        name = file_name or p.name
        ctype = content_type or mimetypes.guess_type(name)[0] or "application/octet-stream"

        return cls(file_name=name, content_type=ctype, content=b64)


class SendifyError(Exception):
    """Base error for Sendify client."""


class SendifyHTTPError(SendifyError):
    """Raised when server returns non-2xx."""


class SendifyRequestError(SendifyError):
    """Raised for networking / request issues."""


class SendifyClient:
    def __init__(self, config: SendifyConfig):
        self._config = config
        self._base_url = self._normalize_host(config.host).rstrip("/")
        self._session = requests.Session()
        self._session.headers.update(
            {
                "Content-Type": "application/json",
                "Authorization": f"Bearer {config.token}",
            }
        )

    @staticmethod
    def _normalize_host(host: str) -> str:
        host = host.strip()
        if host.startswith("http://") or host.startswith("https://"):
            return host
        # Default to https if scheme not provided
        return f"https://{host}"

    def close(self) -> None:
        self._session.close()

    def send_message(
        self,
        *,
        message_type: int,
        sender: Optional[str],
        recipients: List[str],
        body: str,
        subject: Optional[str] = None,
        attachments: Optional[List[Attachment]] = None,
        is_separate: Optional[bool] = None,
        priority: int = 9,
        sending_status: int = 1,
        extra_fields: Optional[JsonDict] = None,
    ) -> JsonDict:
        """
        Convenience wrapper to build the JSON payload.

        Returns parsed JSON response (dict) if possible, otherwise {"status_code": ..., "text": ...}.
        - `attachments` is a list of Attachment instances. Attachment.content is expected
          to be a string (commonly base64).
        - `subject` and `is_separate` map to "Subject" and "IsSeparate" in the API.
        """
        payload: JsonDict = {
            "MessageType": message_type,
            "Sender": sender,
            "Recipients": recipients,
            "Priority": priority,
            "Body": body,
            "IsSeparate": bool(is_separate)
        }

        if sender is None:
            sender = ""

        if sender is not None:
            payload["Sender"] = sender

        if subject is not None:
            payload["Subject"] = subject

        if attachments:
            payload["Attachments"] = [a.to_api_dict() for a in attachments]

        if is_separate is not None:
            payload["IsSeparate"] = bool(is_separate)

        if sending_status is not None:
            payload["SendingStatus"] = sending_status

        if extra_fields:
            payload.update(extra_fields)

        return self.post_message(payload)

    def post_message(self, payload: JsonDict) -> JsonDict:
        """
        Low-level method: POST /api/v1/messages with the given payload.
        """
        url = f"{self._base_url}/api/v1/messages"
        return self._post_with_retries(url=url, json_payload=payload)

    def _post_with_retries(self, *, url: str, json_payload: JsonDict) -> JsonDict:
        last_exc: Optional[Exception] = None

        for attempt in range(self._config.retries + 1):
            try:
                resp = self._session.post(
                    url,
                    json=json_payload,
                    timeout=self._config.timeout,
                    verify=self._config.verify_ssl,
                )

                match resp.status_code:
                    case code if 200 <= code < 300:
                        return self._safe_json(resp)
                    case 429 if attempt < self._config.retries:
                        self._sleep_backoff(attempt)
                        continue
                    case code if 500 <= code < 600 and attempt < self._config.retries:
                        self._sleep_backoff(attempt)
                        continue
                    case _:
                        raise SendifyHTTPError(
                            f"HTTP {resp.status_code} calling {url}: {resp.text}"
                        )

            except (requests.Timeout, requests.ConnectionError) as e:
                last_exc = e
                if attempt < self._config.retries:
                    self._sleep_backoff(attempt)
                    continue
                raise SendifyRequestError(f"Request failed calling {url}: {e}") from e

            except requests.RequestException as e:
                # Other requests-level errors (invalid URL, etc.)
                raise SendifyRequestError(f"Request error calling {url}: {e}") from e

        # Should not reach here; keep as a fallback.
        raise SendifyRequestError(f"Request failed calling {url}: {last_exc}")

    def _sleep_backoff(self, attempt: int) -> None:
        # Exponential-ish backoff: base * (attempt+1)
        time.sleep(self._config.backoff_seconds * (attempt + 1))

    @staticmethod
    def _safe_json(resp: "requests.Response") -> JsonDict:
        try:
            data = resp.json()
            if isinstance(data, dict):
                return data
            return {"data": data, "status_code": resp.status_code}
        except ValueError:
            return {"status_code": resp.status_code, "text": resp.text}
