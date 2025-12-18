# Sendify

Sendify is a lightweight messaging API and client suite for sending SMS/email messages via a hosted Sendify service. This repository contains:
- A .NET 8 API surface and supporting services.
- A minimal Python client (`sendify_client/sendify_client.py`) compatible with Python 3.10+.
- Documentation, contributing guidelines, and an MIT license.

Quick, focused, and production-minded — suitable for scripts, background workers, and service integrations.

## Features
- Minimal Python client with configurable retries, timeout, and SSL verification.
- .NET 8 API models and token service abstractions for secure JWT management.
- Clear defaults for development and production use.

## Requirements
- .NET 8 SDK for building and running the .NET projects.
- Python 3.10+ and `requests` (for the Python client).
  - Install: `pip install requests`

## Quickstart — Python client
1. Install dependency:
   - `pip install requests`
2. Example usage (replace token and host):
