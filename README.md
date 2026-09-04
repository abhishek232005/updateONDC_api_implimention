# Kuvik Loans ONDC BAP — FIS12 Personal Loan (Offline)

Production-oriented ASP.NET Core 10 BAP foundation for ONDC FIS12 v2.0.3. It implements only the offline flow actions: `search`, `on_search`, `select`, `on_select`, `confirm`, `on_confirm`, `status`, and `on_status`.

## Architecture

`Controllers → validation / transaction services → repository or signed HTTP client → EF Core SQLite (configurable)`.

Key folders: `Controllers`, `Models/Common`, `Services` (ONDC context, signing, encryption, validation, transactions), `Data`, `Middleware`, `Configuration`, and `Tests`.

The source of truth used while building this project was the official `draft-FIS12-2.0.3` branch. Payloads preserve the full FIS message as JSON after common context validation; this avoids inventing fields while allowing the exact official xinput/catalog/order structures to pass through unchanged.

## Setup

```powershell
dotnet restore Kuvik.ONDC.slnx
dotnet ef database update --project Kuvik.ONDC.Api --startup-project Kuvik.ONDC.Api
dotnet dev-certs https --trust
dotnet run --project Kuvik.ONDC.Api --launch-profile https
dotnet test Kuvik.ONDC.slnx
```

Set development secrets with .NET User Secrets or deployment environment variables. Copy values from `Kuvik.ONDC.Api/.env.example`; it contains placeholders only. Never commit the real signing key, encryption key, or registry-resolved BPP public keys.

## Endpoints

`POST /ondc/search`, `/ondc/select`, `/ondc/confirm`, and `/ondc/status` validate the FIS context, enforce state, persist the outbound event, sign the raw JSON, and send it to the configured HTTPS target. They return `202 Accepted` only after the remote endpoint accepted the HTTP request.

`POST /ondc/on_search`, `/ondc/on_select`, `/ondc/on_confirm`, and `/ondc/on_status` verify the raw-body Authorization or X-Gateway-Authorization signature before persisting a state change. They return ONDC `ACK` or `NACK` envelopes. `GET /health` exposes no secrets. Swagger is enabled only for Development and PreProduction.

## Security

Outgoing signing follows ONDC subscriber signing: BLAKE2b-512 (`BLAKE-512`) body digest and an Ed25519 signature over `(created)`, `(expires)`, and `digest`. Incoming callbacks require a configured public key resolver mapping; absent, expired, replayed, malformed, or invalid signatures are rejected. In production replace the configuration resolver with an ONDC Registry lookup/cache implementation.

`OndcEncryptionService` is AES-256-GCM for encrypted local application fields. It is deliberately not applied to normal FIS transport payloads: the FIS12 offline endpoint schema does not define a blanket payload-encryption wrapper. Add only the ONDC-prescribed mechanism at the specific field/flow that requires it.

Structured logs contain action, transaction ID, and message ID. They intentionally exclude request bodies, credentials, authorization headers, customer data, and encryption material.

## State management

The allowed progression is `SearchInitiated → SearchReceived → SelectInitiated → SelectReceived → ConfirmInitiated → ConfirmReceived → StatusRequested → StatusReceived`. Invalid transitions fail closed. The database stores identifiers, state, provider/item/application references, timestamps, errors, and the relevant message JSON.

## Deployment and Workbench QA

Before Workbench execution provide: a public HTTPS callback host, registered BAP subscriber ID and callback URI, unique signing key ID/private key, registry public-key resolution, gateway/BPP target URL, database, and production secret store. Validate the exact generated payloads with ONDC’s current Workbench/log validator. This repository has not been run against ONDC Workbench and makes no claim that it has passed.

For a publicly exposed deployment also enforce an upstream authenticated internal API boundary for the four BAP-originated routes, restrict CORS to known frontends, terminate TLS with a valid certificate, apply database migrations through CI/CD, and retain security-reviewed logs.
