# ASP.NET Core MVC Chat (Portfolio)

A small but production-minded **ASP.NET Core 8** chat demo that showcases senior-level engineering practices:

* **MVC** for the chat UI + **Razor Pages** for ASP.NET Identity
* **Server-Sent Events (SSE)** for real-time updates
* **HashiCorp Vault** secrets bootstrap + readiness gate + health checks
* **Antiforgery** protection for AJAX (`/chat/send`)
* **Options pattern** for non-secret configuration
* Clean DI, async I/O, structured logging, and localization

> This project is a portfolio piece: concise app, complete infrastructure patterns.

---

## ✨ Features

* **Real-time chat** via SSE (`/chat/stream`)
* **Post messages** via `POST /chat/send` (CSRF-protected)
* **HashiCorp Vault** integration with background bootstrap/refresh worker
* **Readiness middleware** that short-circuits requests until secrets are loaded
* **Health endpoints**: `/health/live`, `/health/ready`
* **Localization** using `IStringLocalizer` (resources collocated with the marker type)
* **Identity** (Razor Pages) with a scoped `DbContext` created via a factory
* Production-leaning **Docker Compose** setup (**PostgreSQL** + Vault dev + app)

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

* **.NET 8**, ASP.NET Core MVC, Razor Pages (Identity)
* **Entity Framework Core** with **Npgsql** (PostgreSQL provider)
* **PostgreSQL** as DBMS
* SSE for real-time updates
* HashiCorp Vault (dev mode in local compose)
* NLog (or built-in logging), HealthChecks
* Localization via `IStringLocalizer`

---

## 🚀 Getting started

### Prerequisites

* .NET 8 SDK
* PostgreSQL (“/ASP.Net_Core_MVC_Chat_Test/Storage/Migrations” - database description)
* Docker & Docker Compose
* (Optional) `curl` for quick smoke tests

### 1) Clone

```bash
git clone https://github.com/<you>/<repo>.git
cd <repo>
```

### 2) Run with Docker Compose

This brings up:

* **PostgreSQL** (`postgres`)
* **Vault dev** (`vault`)
* **App** (`web`)

```bash
docker compose up --build
```

When all services are healthy:

* App: [http://localhost:5206](http://localhost:5206)
* Health:

  * Liveness → `GET http://localhost:5206/health/live`
  * Readiness → `GET http://localhost:5206/health/ready`

> The app stays “not ready” until secrets are loaded from Vault (the worker loads them and then signals readiness).

---

## 🔐 Secrets (Vault dev)

The compose file initializes Vault in dev mode with a known root token and a KV (v2) mount at `secret/`.
Write your app’s secrets to `secret/myapp/config`:

```bash
# Write secrets into Vault dev (adjust values as needed)
docker exec -it vault \
  vault kv put secret/myapp/config \
  DatabaseConnection="Host=postgres;Port=5432;Database=ChatDb;Username=postgres;Password=YourStrongPass" \
  EmailSmtpHost="smtp.example.com" \
  EmailSmtpPort="587" \
  EmailAddress="no-reply@example.com" \
  EmailPassword="p@ssw0rd"
```

**Notes**

* PostgreSQL connection string uses `Host=`, not `Server=`.
* In production, **do not** store tokens in appsettings; use env vars / workload identity.

---

## 🧪 Local run without Docker

1. Start a PostgreSQL instance (local or container).
2. Export Vault credentials:  
   * env vars:
     ** `HashicorpVaultCredentialToken`, `HashicorpVaultCredentialAddress`, `HashicorpVaultCredentialMountPoint` and `HashicorpVaultCredentialPath`
   * user-secrets in Dev: 
     ** `HashicorpVault` section
     ** `Token`, `Address`, `MountPoint` and `Path` keys

3. Run:

   ```bash
   dotnet run --project src/MVC_SSL_Chat
   ```

If you’re using EF Core migrations locally, ensure the `Npgsql.EntityFrameworkCore.PostgreSQL` package is referenced, then:

```bash
dotnet ef database update
```

---

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
  body: JSON.stringify({ Content: "Hello!" }),
  credentials: "include"
});
```

> If you cross origins/ports locally, ensure CORS and set antiforgery cookie `SameSite=None; Secure=true` (use HTTPS).

---

## ❤️ Health endpoints

* **Liveness**: `GET /health/live` → always healthy if process is responsive
* **Readiness**: `GET /health/ready` → healthy **after secrets are loaded**

Use in K8s:

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

* **Unit**: `ChatMessageBatchWriter`, `MessageStreamService`, secrets readiness gate, confirmation email content builder.
* **Integration**: Identity sign-up/sign-in (SQLite-in-mem or Testcontainers PostgreSQL), `/chat/send` (CSRF pass/fail), `/chat/stream`.
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

```
src/
  MVC_SSL_Chat/
    Controllers/
      ChatController.cs
    Pages/ (Identity)
    Services/
      MessageStreamService.cs
      ChatMessageBatchWriter.cs
      Email/
      Users/
    Middleware/
      SecretsReadinessGateMiddleware.cs
    Hosted/
      VaultSecretsLoadWorker.cs
    Resources/
      Contracts.Resources.Localization.resx
    wwwroot/                # (add your static assets)
tests/
  Unit/
  Integration/
docker/
  docker-compose.yml
  Dockerfile
```

---

## 🐳 Docker Compose (example)

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: YourStrongPass
      POSTGRES_DB: ChatDb
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d ChatDb"]
      interval: 10s
      timeout: 3s
      retries: 10

  vault:
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

  web:
    build:
      context: .
      dockerfile: docker/Dockerfile
    environment:
      ASPNETCORE_URLS: "http://+:8080"
      # Vault endpoint (non-secrets)
      HASHICORP_VAULT_ADDRESS: "http://vault:8200"
      HASHICORP_VAULT_MOUNTPOINT: "secret"
      HASHICORP_VAULT_PATH: "myapp/config"
      # Token ONLY for dev; never commit real tokens
      HASHICORP_VAULT_TOKEN: "root"
    depends_on:
      postgres:
        condition: service_healthy
      vault:
        condition: service_healthy
    ports:
      - "5206:8080"
```

---

## 🧭 Roadmap (optional polish)

* Replace `ConcurrentBag` with `Channel<ChatMessage>` for backpressure
* Prometheus metrics (SSE clients, batch flush duration, DB errors)
* Swagger/OpenAPI for `/chat/send`
* CI: build/test/publish (GitHub Actions)

---

## 📄 License

MIT (or your preferred license)

---

## 🙌 Acknowledgements

* ASP.NET Core team & docs
* HashiCorp Vault (dev mode)
* PostgreSQL & Npgsql

---

👤 Author
Sergey Korolkov
[LinkedIn](https://www.linkedin.com/in/sergey-korolkov-518508a4/) • [GitHub](https://github.com/emailfansendtest-sketch)




