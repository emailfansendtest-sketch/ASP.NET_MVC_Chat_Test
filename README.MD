# MVC SSL Chat

A portfolio ASP.NET Core MVC chat application built to explore real-time delivery, concurrent message processing, persistence, runtime secret management, and application operability.

The project demonstrates:

- real-time message delivery with Server-Sent Events (SSE);
- bounded asynchronous buffering for live delivery and persistence;
- background batch persistence to PostgreSQL;
- runtime loading and refresh of database and SMTP secrets from HashiCorp Vault;
- application readiness gating while required secrets are unavailable;
- structured logging with Serilog;
- ASP.NET Core Identity and email confirmation;
- localization with `IStringLocalizer`;
- a containerized development/demo environment using Docker Compose.

> **Why Vault in a portfolio project?**  
> Vault is used here to demonstrate runtime secret loading rather than storing database and SMTP credentials in application configuration. The application distinguishes between non-sensitive configuration and sensitive values loaded from an external secret store.

---

## Architecture

```text
Web — MVC_SSL_Chat
├── MVC controllers
├── Razor Pages / ASP.NET Core Identity
├── SSE response writer
├── health-check endpoints
├── antiforgery configuration
├── Serilog configuration
└── SecretsReadinessGateMiddleware
    └── returns 503 for normal application requests
        until initial Vault secrets are available

Application
├── MessageStreamService
│   ├── replays recent persisted messages
│   ├── subscribes clients to live chat events
│   └── writes SSE keep-alives
│
├── ChatEventBus / ChatEventSubscriber
│   └── bounded per-client queues for live events
│
├── MessageBatchWriterService
│   ├── bounded persistence queue
│   └── batch extraction and database persistence
│
├── MessageBatchWriterWorker
│   └── periodic background flushing
│
├── user/current-user services
└── localized email-confirmation content

Storage
├── EF Core
├── PostgreSQL / Npgsql
├── ASP.NET Core Identity storage
├── EF Core migrations
└── database retry handling

SecuritySupplements
├── VaultCredentialsResolver
├── HashicorpVaultClient
├── VaultSecretsLoadWorker
└── SecretsReadinessTracker

Email
└── SMTP email sender

Infrastructure / runtime
├── PostgreSQL
├── HashiCorp Vault
├── MailHog
└── Docker Compose
```

---

## Technology stack

- **.NET 8**
- **ASP.NET Core MVC**
- **Razor Pages / ASP.NET Core Identity**
- **Entity Framework Core**
- **Npgsql / PostgreSQL**
- **Server-Sent Events (SSE)**
- **System.Threading.Channels**
- **HashiCorp Vault / VaultSharp**
- **Serilog**
- **Polly**
- **EFCore.BulkExtensions**
- **Docker / Docker Compose**
- **MailHog**
- **Localization via `IStringLocalizer`**

---

## Message delivery

The application separates live message delivery from persistence.

### Live SSE delivery

Connected clients receive messages using Server-Sent Events.

Each subscriber has its own bounded `Channel<ChatMessageDto>`. The subscriber queue uses:

```csharp
BoundedChannelFullMode.DropOldest
```

This prevents a slow SSE client from blocking message publication for other connected clients.

When a client connects, `MessageStreamService` first retrieves persisted messages from the configured replay interval and then keeps the connection alive while the client remains connected.

The default configuration is:

```json
"MessageStream": {
  "KeepAliveIntervalMs": "15000",
  "ReplayLookbackS": "86400"
}
```

The replay interval therefore defaults to the previous 24 hours.

### Persistence

Messages intended for database persistence are written to a separate bounded channel.

Unlike the per-client SSE queues, the persistence channel uses:

```csharp
BoundedChannelFullMode.Wait
```

This applies backpressure when the persistence queue reaches capacity instead of deliberately discarding queued messages.

The relevant defaults are:

```json
"MessageWriter": {
  "MessageQueueCapacity": "10000",
  "MessageBatchSize": "256"
},
"Persistence": {
  "FlushIntervalMs": "1000"
}
```

`MessageBatchWriterWorker` periodically asks `MessageBatchWriterService` to persist buffered messages in batches.

Database writes use retry handling configured through:

```json
"DatabaseClient": {
  "MaximumRetryCount": "5",
  "IntervalBetweenRetriesMs": "1000"
}
```

---

## HashiCorp Vault

Sensitive database and SMTP configuration is loaded from HashiCorp Vault at runtime.

The application currently expects the following Vault values:

- `DatabaseConnection`
- `EmailSmtpHost`
- `EmailSmtpPort`
- `EmailAddress`
- `EmailPassword`
- `EmailDisplayName`

The Docker Compose environment seeds them under:

- **mount:** `secret`
- **path:** `chatapp`

### Initial loading and refresh

`VaultSecretsLoadWorker` performs the initial Vault read in the background.

Until the first successful load completes:

- the application process is running;
- health-check endpoints remain accessible;
- ordinary application requests are intercepted by `SecretsReadinessGateMiddleware` and receive HTTP `503`.

After the initial secrets load succeeds, the readiness tracker is signaled and normal request processing is allowed.

Vault values can also be periodically refreshed. The default configuration is:

```json
"SecretsLoader": {
  "InitialReadIntervalMs": "1000",
  "RefreshReadIntervalMs": "600000"
}
```

With this configuration, an unsuccessful initial load is retried after one second and successfully loaded secrets are subsequently refreshed every ten minutes.

Setting `RefreshReadIntervalMs` to `0` disables periodic refresh after the initial load.

---

## Vault credentials

How credentials used to access Vault are obtained depends on the application environment.

### Development

When `IHostEnvironment.IsDevelopment()` is true, `VaultCredentialsResolver` reads the `HashicorpVault` configuration section.

For local development, these values can be stored with .NET User Secrets:

```text
HashicorpVault:Token
HashicorpVault:Address
HashicorpVault:MountPoint
HashicorpVault:Path
```

For a Vault instance matching the Docker Compose configuration, the corresponding values are:

```text
Token      = root
Address    = http://localhost:8200
MountPoint = secret
Path       = chatapp
```

For example:

```bash
dotnet user-secrets set "HashicorpVault:Token" "root" --project MVC_SSL_Chat
dotnet user-secrets set "HashicorpVault:Address" "http://localhost:8200" --project MVC_SSL_Chat
dotnet user-secrets set "HashicorpVault:MountPoint" "secret" --project MVC_SSL_Chat
dotnet user-secrets set "HashicorpVault:Path" "chatapp" --project MVC_SSL_Chat
```

### Non-Development environments

Outside `Development`, Vault credentials are obtained from environment variables:

```text
HashicorpVaultCredentialToken
HashicorpVaultCredentialAddress
HashicorpVaultCredentialMountPoint
HashicorpVaultCredentialPath
```

`docker-compose.yml` supplies these variables to the web container.

---

## Docker Compose environment

The supplied Compose configuration defines four services.

### PostgreSQL

The `postgres` service provides the application's PostgreSQL database.

It uses a persistent Docker volume:

```text
postgres_data
```

The database port is intentionally not exposed to the host by default.

### Vault

The `vault` service runs HashiCorp Vault in development mode.

It listens on port:

```text
8200
```

and uses the development root token:

```text
root
```

Development mode is used only to make the portfolio environment self-contained. It is not intended as a production Vault configuration.

### Vault initialization

`vault-init` is a one-shot container.

After Vault becomes healthy, it:

1. writes the application's database and SMTP values to `secret/chatapp`;
2. reads the values back to verify that the write succeeded;
3. exits successfully.

The web service depends on successful completion of this initialization step.

### MailHog

MailHog acts as the development SMTP service.

Its default ports are:

- `1025` — SMTP;
- `8025` — web inbox.

### Web application

The Compose configuration starts the web application with:

```text
ASPNETCORE_ENVIRONMENT=Production.Docker
```

and overrides the container URL with:

```text
ASPNETCORE_URLS=http://+:8080
```

The application is published to the host on:

```text
http://localhost:8080
```

The Dockerfile itself contains a demo self-signed HTTPS configuration for port `8443`, but the supplied Docker Compose environment overrides that configuration and runs the application over HTTP on port `8080`.

---

## Database migrations

EF Core migrations are stored in:

```text
Storage/Migrations
```

Database schema migration is **not currently performed automatically by `docker-compose.yml` or application startup**.

The migration history therefore represents the application's schema, but schema deployment remains a separate development/deployment concern in the current version of the project.

---

## Health and readiness

The application exposes two health-check endpoints.

### Liveness

```text
/health/live
```

This reports basic process/application liveness.

### Readiness

```text
/health/ready
```

The current readiness check represents **Vault-secret readiness**.

It reports ready after the initial set of required secrets has been loaded successfully.

Database connectivity is not currently part of the readiness check.

Normal application requests are additionally protected by `SecretsReadinessGateMiddleware`, which returns HTTP `503` while the secrets readiness tracker is not ready.

---

## Logging

The application uses **Serilog** with configuration selected according to the environment.

### Development

`appsettings.Development.json` configures:

- human-readable console output;
- compact JSON file logging under `logs/`;
- daily rolling log files.

### Production

`appsettings.Production.json` configures:

- console logging;
- compact JSON rolling files.

### Production.Docker

`appsettings.Production.Docker.json` writes compact JSON logs to the console, allowing the container runtime to collect and route application output.

Logs are enriched with information including:

- environment name;
- machine name;
- process ID;
- thread ID;
- application name.

---

## HTTPS behavior

HTTPS redirection is controlled through:

```text
HttpsRedirection:Enabled
```

or the equivalent environment variable:

```text
HttpsRedirection__Enabled
```

The default application configuration enables HTTPS redirection.

The supplied Docker Compose environment explicitly sets:

```text
HttpsRedirection__Enabled=false
```

because Compose exposes the application over HTTP on port `8080`.

The Dockerfile also contains a self-signed certificate configuration for a standalone container HTTPS endpoint on `8443`; this configuration is overridden by the supplied Compose configuration.

---

## Localization

Localization resources are stored under the configured `Resources` path and consumed through ASP.NET Core's `IStringLocalizer`.

Localized content is also used when generating email-confirmation messages.

---

## Configuration

Non-sensitive application behavior is controlled through strongly typed options.

The main configuration groups currently include:

```text
DatabaseClient
SecretsLoader
MessageStream
Persistence
MessageWriter
ChatEvent
HttpsRedirection
```

Sensitive database and SMTP values are intentionally loaded separately from Vault.

---

## Troubleshooting

### Vault CLI reports `HTTP response to HTTPS client`

Vault development mode uses HTTP.

Set the Vault CLI address explicitly:

```bash
export VAULT_ADDR=http://127.0.0.1:8200
export VAULT_TOKEN=root
```

Then, for example:

```bash
vault kv get -mount=secret chatapp
```

When using Docker Compose:

```bash
docker compose exec vault sh -lc \
  'export VAULT_ADDR=http://127.0.0.1:8200 VAULT_TOKEN=root; vault kv get -mount=secret chatapp'
```

### Vault values disappear after recreating the container

Vault runs in development/in-memory mode.

Its data is therefore not persistent. Recreating the Vault container removes the stored secrets.

The `vault-init` service seeds `secret/chatapp` again when the Compose environment is recreated.

### PostgreSQL port conflict

The supplied Compose file does **not** expose PostgreSQL port `5432` to the host.

If host access is required, a mapping such as:

```yaml
ports:
  - "5433:5432"
```

can be added.

### Application returns HTTP 503

During startup, ordinary application routes return `503` until the initial Vault secret load succeeds.

Check:

- Vault availability;
- Vault credentials;
- mount and path configuration;
- application logs;
- `/health/ready`.

---

## Project scope

This repository is a portfolio and learning project rather than a production deployment template.

Its focus is demonstrating application-level concerns including:

- real-time communication;
- bounded asynchronous processing;
- persistence batching;
- dependency readiness;
- external secret management;
- configuration;
- structured logging;
- health checks;
- containerized infrastructure.

The supplied Vault configuration deliberately uses development mode, and database migrations are not currently automated as part of application or Compose startup.