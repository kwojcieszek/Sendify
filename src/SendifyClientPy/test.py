from sendify_client import SendifyClient, SendifyConfig, Attachment
from datetime import datetime

# Global sensitive configuration (replace placeholders before running)
SENDIFY_HOST = "https://sendify.test.com"  # e.g. "https://sendify.test.com"
SENDIFY_TOKEN = "<REDACTED_TOKEN>"  # PLACEHOLDER: set your real token here
SMS_RECIPIENTS = []  # e.g. ["48600600600"]
SMS_SENDER = ""  # optional, e.g. "600601601"
EMAIL_RECIPIENTS = []  # e.g. ["user@example.com"]
EMAIL_SENDER = "mailer@example.com"
ATTACHMENT_PATH = ""  # e.g. r"c:\sample-local-pdf.pdf"


if __name__ == "__main__":
    if not SENDIFY_TOKEN or SENDIFY_TOKEN == "<REDACTED_TOKEN>":
        raise SystemExit("Set SENDIFY_TOKEN global variable at top of this file before running")

    cfg = SendifyConfig(
        host=SENDIFY_HOST,
        token=SENDIFY_TOKEN,
        timeout=15.0,
        retries=2,
    )
    client = SendifyClient(cfg)

    try:
        # SMS
        if not SMS_RECIPIENTS:
            print("Skipping SMS send: update SMS_RECIPIENTS global variable")
        else:
            result = client.send_message(
                message_type=1,
                sender=SMS_SENDER,
                recipients=SMS_RECIPIENTS,
                priority=9,
                body=f"Test: {datetime.now()}",
            )
            print("OK (SMS):", result)

        # Email
        if not EMAIL_RECIPIENTS:
            print("Skipping email send: update EMAIL_RECIPIENTS global variable")
        else:
            attachments = [Attachment.from_file(ATTACHMENT_PATH)] if ATTACHMENT_PATH else None

            result = client.send_message(
                message_type=2,
                sender=EMAIL_SENDER,
                recipients=EMAIL_RECIPIENTS,
                priority=9,
                is_separate=False,
                body="Test email",
                attachments=attachments,
            )
            print("OK (Email):", result)
    finally:
        client.close()