"""
sendify_client.py

Minimal client for:
POST {host}/api/v1/messages
Content-Type: application/json
Authorization: Bearer <token>

Example payload:
{
  "MessageType": 1,
  "Recipients": ["48602174021"],
  "Priority": 9,
  "Body": "Test ..."
}
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Dict, List, Optional
import time

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
        recipients: List[str],
        body: str,
        priority: int = 9,
        sending_status: int = 1,
        extra_fields: Optional[JsonDict] = None,
    ) -> JsonDict:
        """
        Convenience wrapper to build the JSON payload.

        Returns parsed JSON response (dict) if possible, otherwise {"status_code": ..., "text": ...}.
        """
        payload: JsonDict = {
            "MessageType": message_type,
            "Recipients": recipients,
            "Priority": priority,
            "Body": body
        }
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
