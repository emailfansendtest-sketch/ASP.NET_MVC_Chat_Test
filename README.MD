# ASP.NET Core MVC Chat (Portfolio)

A small but production-minded **ASP.NET Core 8** chat demo that showcases senior-level engineering practices:

- **MVC** for the chat UI + **Razor Pages** for ASP.NET Identity
- **Server-Sent Events (SSE)** for real-time updates
- **HashiCorp Vault** secrets bootstrap + readiness gate + health checks (**required**)
- **Antiforgery** protection for AJAX (`/chat/send`)
- **Options pattern** for non-secret configuration
- Clean DI, async I/O, structured logging, and localization

> Portfolio note: the app is intentionally small, but the infrastructure patterns are realistic.

---

## ⚠️ Vault is required

This application **does not start "ready" without Vault**:
- DB and SMTP settings are loaded from Vault by a background worker.
- Database access blocks behind a readiness gate until secrets are loaded.

If you want "no-Vault local mode", that would be a **code change** (a dev fallback) and is not implemented.

---

## ✨ Features

- **Real-time chat** via SSE (`/chat/stream`)
- **Post messages** via `POST /chat/send` (CSRF-protected)
- **HashiCorp Vault** integration with background bootstrap/refresh worker
- **Readiness middleware** that short-circuits requests until secrets are loaded
- **Health endpoints**: `/health/live`, `/health/ready`
- **Localization** using `IStringLocalizer` (resources collocated with the marker type)
- **Identity** (Razor Pages) with a scoped `DbContext` created via a factory

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

---

## 🚀 Getting started (recommended local dev)

This path runs **PostgreSQL + Vault** in Docker, and runs the **ASP.NET app on your machine**.

### Prerequisites

- .NET 8 SDK
- Docker + Docker Compose
- (Optional) `curl` for smoke tests
- `dotnet-ef` tool if you want to apply migrations from CLI:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 1) Clone

```bash
git clone https://github.com/emailfansendtest-sketch/ASP.NET_MVC_Chat_Test.git
cd ASP.NET_MVC_Chat_Test
```

### 2) Start PostgreSQL + Vault (dev) with Docker Compose

Create a file docker-compose.yml in the repo root with the content below, then run:

```bash
docker compose up -d
```

Compose file (deps only):

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: YourPostgresUserName
      POSTGRES_PASSWORD: YourStrongPass
      POSTGRES_DB: ChatDb
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U YourPostgresUserName -d ChatDb"]
      interval: 10s
      timeout: 3s
      retries: 10

  vault:
    container_name: RepoVault1
    image: hashicorp/vault:1.16
    environment:
      VAULT_DEV_ROOT_TOKEN_ID: "root"
      VAULT_ADDR: "http://0.0.0.0:8200"
    command: ["server", "-dev", "-dev-root-token-id=root", "-dev-listen-address=0.0.0.0:8200"]
    ports:
      - "8200:8200"
    healthcheck:
      test: ["CMD", "wget", "-qO-", "http://localhost:8200/v1/sys/health" ]
      interval: 10s
      timeout: 3s
      retries: 10
```

### 3) Write secrets into Vault

The app expects a KV v2 secret under:

* MountPoint: secret

* Path: myapp/config

Write it using:

```bash
docker exec -it RepoVault1 \
  vault kv put secret/myapp/config \
  DatabaseConnection="Host=localhost;Port=5432;Database=ChatDb;Username=YourPostgresUserName;Password=YourStrongPass" \
  EmailSmtpHost="smtp.example.com" \
  EmailSmtpPort="587" \
  EmailAddress="no-reply@example.com" \
  EmailPassword="p@ssw0rd" \
  EmailDisplayName="Chat App"
```

Required keys

* DatabaseConnection

* EmailSmtpHost

* EmailSmtpPort

* EmailAddress

* EmailPassword

Optional:

* EmailDisplayName

**Notes**

* PostgreSQL connection string uses `Host=`, not `Server=`.

---

### 4) Configure Vault credentials for the app (Development)

In Development, the app reads Vault credentials from user-secrets under the HashicorpVault section:

```bash
cd MVC_SSL_Chat
dotnet user-secrets init
dotnet user-secrets set "HashicorpVault:Token" "root"
dotnet user-secrets set "HashicorpVault:Address" "http://localhost:8200"
dotnet user-secrets set "HashicorpVault:MountPoint" "secret"
dotnet user-secrets set "HashicorpVault:Path" "myapp/config"
cd ..
```

### 5) Apply EF Core migrations

From repo root:

```bash
dotnet ef database update --project Storage --startup-project MVC_SSL_Chat
```

### 6) Run the app

```bash
dotnet run --project MVC_SSL_Chat
```

Open:

App: https://localhost:xxxx/ (see console output)

Health:

GET /health/live

GET /health/ready 

> The app stays "not ready" until secrets are loaded from Vault (the worker loads them and then signals readiness).

---

## 🔐 Vault credentials (Production / non-Dev)

Outside Development, credentials are read from environment variables:

* HashicorpVaultCredentialToken

* HashicorpVaultCredentialAddress

* HashicorpVaultCredentialMountPoint

* HashicorpVaultCredentialPath

Example:

```bash
export HashicorpVaultCredentialToken="..."
export HashicorpVaultCredentialAddress="http://vault:8200"
export HashicorpVaultCredentialMountPoint="secret"
export HashicorpVaultCredentialPath="myapp/config"
```

> Production note: never commit real tokens. Use workload identity / secure secret injection.

## 🔁 Real-time & antiforgery

* **SSE**: The browser connects to `/chat/stream` and receives messages as `data:` payloads.
* **Send**: The JS posts to `/chat/send` with a CSRF header.
  The app issues a JS-readable cookie `XSRF-TOKEN` on GET; the client reads it and sends `X-CSRF-TOKEN`.

```js
function readCookie(name) {
  const m = document.cookie.match(new RegExp('(^|;\\s*)' + name + '=([^;]*)'));
  return m ? decodeURIComponent(m[2]) : null;
}

const token = readCookie("XSRF-TOKEN");
await fetch("/chat/send", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    "X-CSRF-TOKEN": token
  },
  body: JSON.stringify({ content: "Hello!" })
});
```

## ❤️ Health endpoints

* **Liveness**: `GET /health/live` → always healthy if process is responsive
* **Readiness**: `GET /health/ready` → healthy **after secrets are loaded**

```yaml
livenessProbe:
  httpGet: { path: /health/live, port: 8080 }
readinessProbe:
  httpGet: { path: /health/ready, port: 8080 }
```

---

## 🌍 Localization

Resources live in `Resources/` with filenames matching the **fully-qualified name** of a marker type, for example:

```
/Resources/Contracts.Resources.Localization.resx
/Resources/Contracts.Resources.Localization.ru.resx
```

Marker:

```csharp
namespace Contracts.Resources
{
    public sealed class Localization { }
}
```

Usage:

```csharp
public class ConfirmationEmailBuilder(IStringLocalizer<Contracts.Resources.Localization> loc) { ... }
```

---

## 🧪 Tests (suggested)

* **Unit**: batch writer service, secrets readiness gate, confirmation email content builder.
* **Integration**: `/chat/send` (CSRF pass/fail), `/chat/stream`, Identity flows.
* **Functional**: `WebApplicationFactory` that connects to SSE and posts a message.

---

## 🧯 Security notes

* CSRF protection on `/chat/send` ✅
* Add **rate limiting** to `/chat/send` if you expect abuse
* Use **HTTPS** whenever possible
* **HSTS** enabled in non-Dev
* Consider a **CSP** header (e.g., `default-src 'self'; script-src 'self'`)
* Never log secrets; scrub connection strings in logs

---

## 🗂️ Project structure (example)

```bash
ASP.NET_MVC_Chat_Test.sln
MVC_SSL_Chat/            # Web app (MVC + Identity UI)
Application/             # App services: streaming, sending, utilities
Storage/                 # EF Core DbContext + migrations
SecuritySupplements/     # Vault integration + readiness tracker
Contracts/               # Interfaces + options + shared resources
DomainModels/            # Domain entities (ChatMessage, ChatUser, ...)
Email/                   # Email sender + DI wiring
```

---

## 🐳 Docker Compose (example)

This repo currently provides compose examples inline (not committed as files).
If you want to run the web app itself inside Docker as well, you’ll need to add a `Dockerfile` and update compose accordingly.

(Keeping the "deps only" compose above is usually enough for local dev.)

---

## 📄 License

MIT (or your preferred license)

---

👤 Author
Sergey Korolkov
[LinkedIn](https://www.linkedin.com/in/sergey-korolkov-518508a4/) • [GitHub](https://github.com/emailfansendtest-sketch)



