from sendify_client import SendifyClient
from sendify_client import SendifyConfig

if __name__ == "__main__":
    # Example usage (replace token/host):
    cfg = SendifyConfig(
        host="https://sendify.test.com",
        token="YOUR_TOKEN",
        timeout=15.0,
        retries=2,
    )
    client = SendifyClient(cfg)

    #sms
    try:
        result = client.send_message(
            message_type=1,
            recipients=["48600600600"],
            priority=9,
            body="Test"
        )
        print("OK:", result)
    finally:
        client.close()

    #email
    try:
        result = client.send_message(
            message_type=2,
            recipients=["test@gmail.com"],
            priority=9,
            body="Test email"
            )
        print("OK:", result)
    finally:
        client.close()