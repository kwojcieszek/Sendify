# Sendify

Sendify is a lightweight messaging API and client suite for sending **SMS and email messages** via a hosted Sendify service.

It is designed to be **quick, focused, and production-minded**, making it suitable for scripts, background workers, and service-to-service integrations.

---

## Overview

This repository contains:

* A **.NET 8 API** with supporting services and abstractions.
* A **minimal Python client** (`sendify_client/sendify_client.py`) compatible with Python 3.10+.
* Documentation, contribution guidelines, and an **MIT license**.

Sendify prioritizes simplicity, explicit configuration, and predictable runtime behavior.

---

## Features

* Lightweight Python client with:

  * Configurable retries
  * Request timeout
  * SSL verification control
* .NET 8 API with:

  * Clear API models
  * Token service abstractions for secure JWT handling
* MongoDB-backed persistence using a NoSQL data model
* Sensible defaults for development and production environments

---

## Architecture

* **API Layer**: .NET 8 Web API exposing message-sending endpoints
* **Persistence**: MongoDB (NoSQL)
* **Clients**: Minimal Python client using HTTP + JSON
* **Authentication**: JWT-based access tokens

---

## Data Storage

Sendify uses a **NoSQL database (MongoDB)** for persistence.

### Why MongoDB

* Flexible schema for message payloads and provider-specific metadata
* High write throughput, suitable for messaging and outbox-style workloads
* Built-in support for replication, indexing, and TTL policies
* Operational simplicity in containerized and hosted environments

### Typical Stored Data

* Message payloads (SMS / email)
* Delivery status and timestamps
* Provider responses and metadata
* Correlation and tracking identifiers

---

## Requirements

### Backend

* **.NET 8 SDK** (for building and running the API)
* **MongoDB** (local or hosted)

### Python Client

* **Python 3.10+**
* **requests**

Install Python dependency:

```bash
pip install requests
```

---

## Configuration

Configuration is typically provided via environment variables or application settings.

### Common MongoDB Settings

```text
MONGODB_URI=mongodb://localhost:27017
MONGODB_DATABASE=sendify
```

### Authentication

* The API uses **JWT tokens** for authentication.
* Token acquisition and validation are handled via dedicated service abstractions in the .NET layer.

---

## Quickstart — Python Client

### 1. Install dependency

```bash
pip install requests
```

### 2. Example usage

```python
from sendify_client import SendifyClient

client = SendifyClient(
    host="https://api.sendify.local",
    token="YOUR_ACCESS_TOKEN",
    timeout=10,
    retries=3,
    verify_ssl=True
)

response = client.send_message(
    to="+1234567890",
    subject="Test message",
    body="Hello from Sendify"
)

print(response)
```

Replace `host` and `token` with values appropriate for your environment.

---

## Development

### Running the API locally

1. Ensure MongoDB is running
2. Configure required environment variables
3. Build and run the API using the .NET 8 SDK

```bash
dotnet build
dotnet run
```

### Python Client Development

The Python client is intentionally minimal and dependency-light.
It is suitable for:

* Cron jobs
* Background workers
* Automation scripts
* Service integrations

---

## Contributing

Contributions are welcome.

Please ensure that:

* Code is clean and production-ready
* Public APIs remain minimal and explicit
* Changes are documented where appropriate

See `CONTRIBUTING.md` for details.

---

## License

This project is licensed under the **MIT License**.
See the `LICENSE` file for full text.

---

If you want, I can also:

* add an **API endpoint section** (routes, request/response examples),
* create a **separate README for the Python client**, or
* align this README with an existing repo structure (folders, solution names).
