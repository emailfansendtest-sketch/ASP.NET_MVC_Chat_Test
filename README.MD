# MVC SSL Chat (Portfolio Demo)

A small ASP.NET Core MVC chat application showcasing:
- SSE-based chat stream
- background batching/persistence
- readiness gating until sensitive configuration is loaded from HashiCorp Vault
- structured logging via Serilog
- a runnable local stack via Docker Compose (Postgres + Vault + MailHog)

> **Why Vault in a portfolio?**  
> It demonstrates “real-world” configuration handling (DB/SMTP secrets) and the ability to run the app in containerized environments.

---

## 🧱 Architecture (high level)

```
Web (MVC + Razor Pages)
 ├── Controllers (ChatController thin)
 ├── Pages (Identity)
 ├── Middleware: SecretsReadinessGate (short-circuits until secrets ready)
 └── Services
     ├── MessageStreamService (SSE)
     ├── ChatMessageBatchWriter (async buffering & periodic DB flush)
     ├── CurrentUser accessor/service
     └── Email confirmation content (localizer-backed)
 
Background
 └── VaultSecretsLoadWorker (bootstrap + refresh; signals readiness)

Infra
 ├── Health checks (/health/live, /health/ready)
 ├── Options pattern (non-secrets)
 └── HashiCorp Vault provider (secrets at runtime)
```

---

## 🧰 Tech stack

- **.NET 8**, ASP.NET Core MVC, Razor Pages (Identity)
- **Entity Framework Core** with **Npgsql** (PostgreSQL provider)
- **PostgreSQL** as DBMS
- SSE for real-time updates
- HashiCorp Vault (dev mode recommended locally)
- NLog + built-in logging abstractions, HealthChecks
- Localization via `IStringLocalizer`



## Quickstart (Docker Compose)

### Prerequisites
- Docker Desktop / Docker Engine + Docker Compose v2

### Run
From the solution folder (where `docker-compose.yml` is located):

```bash
docker compose up --build
```

### URLs
- App: http://localhost:8080
- MailHog (SMTP inbox UI): http://localhost:8025
- Vault UI: http://localhost:8200/ui

### What Compose does
- Starts **Postgres**
- Starts **Vault in dev mode** (HTTP on 8200)
- Runs a one-shot **vault-init** container that:
  - writes required secrets to Vault (mount `secret/`, path `chatapp`)
  - verifies secrets exist by reading them back
- Starts the **web** container which:
  - reads Vault credentials from environment variables
  - loads secrets into `IOptionsMonitorCache<>`
  - signals readiness and begins serving traffic on port **8080**

---

## Vault secrets structure

Compose seeds these keys under:
- **MountPoint:** `secret`
- **Path:** `chatapp`

Keys:
- `DatabaseConnection`
- `EmailSmtpHost`
- `EmailSmtpPort`
- `EmailAddress`
- `EmailPassword`
- `EmailDisplayName` (optional)

### Verify secrets from your host
```bash
docker compose exec vault sh -lc 'export VAULT_ADDR=http://127.0.0.1:8200 VAULT_TOKEN=root; vault kv get -mount=secret chatapp'
```

### Vault UI login
Vault dev mode uses a static root token:
- Token: `root`

---

## Running locally (Visual Studio)

### Development mode behavior
In `Development`, the app resolves Vault credentials from **User Secrets** (not environment variables).  
In Docker/Production, it reads credentials from **environment variables**.

If you want local Vault usage, set these user secrets:

- `HashicorpVaultCredentialToken`
- `HashicorpVaultCredentialAddress`
- `HashicorpVaultCredentialMountPoint`
- `HashicorpVaultCredentialPath`

Example values (matching Docker):
- Token: `root`
- Address: `http://localhost:8200`
- MountPoint: `secret`
- Path: `chatapp`

---

## Logging (Serilog)

The app uses Serilog with environment-specific configuration:
- `Production.Docker` prints structured logs to console (ideal for containers)

> If you see “no logs” in Docker, ensure:
> - `ASPNETCORE_ENVIRONMENT=Production.Docker`
> - your `appsettings.Production.Docker.json` contains a Serilog `WriteTo` sink (Console)

---

## HTTPS redirection in Docker

The Docker setup runs **HTTP inside the container** (recommended for local compose).  
`UseHttpsRedirection()` is controlled by a config flag:

- `HttpsRedirection:Enabled` (or `HttpsRedirection__Enabled` env var)

Compose sets it to `false` so you don’t need self-signed certs.

---

## Troubleshooting

### Vault CLI shows “HTTP response to HTTPS client”
You’re using the CLI default HTTPS address. Set:

```bash
export VAULT_ADDR=http://127.0.0.1:8200
export VAULT_TOKEN=root
```

### Postgres port conflict (5432 already in use)
This compose file intentionally does **not** publish 5432 to your host.
If you add `ports: "5432:5432"` and something already uses it, change to `"5433:5432"` or remove the mapping.

### Secrets “disappear”
Vault is running in **dev in-memory** mode. If the Vault container is recreated, secrets are gone.
Re-running compose will re-seed them via `vault-init`.

---

## Notes
This repo is intended as a portfolio project. The Docker stack is optimized for “works in one command” local usage and demonstration, not production hardening.
