# ECommerceSpareParts

Backend solution for an e-commerce spare parts platform.

The system is built as a set of .NET services around product catalog, storage operations, purchases, sales, balances,
pricing, search, and analytics. It is intended for a business domain where spare parts are bought, stored, priced,
searched, sold, and analyzed.

## Tech Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Redis
- RabbitMQ / MassTransit
- MinIO, S3-compatible object storage
- Docker Compose
- Loki, Prometheus, Grafana
- xUnit and Testcontainers

## Repository Structure

```text
src/
  Core/
    Abstractions
    Attributes
    Contracts
    Domain
    Enums
    Exceptions
    Extensions
    Utils

  Infrastructure/
    Cache
    Integrations
    Localization
    Mail
    Persistence
    RabbitMq
    Security

  Services/
    Main
    Search
    Pricing
    Analytics
    Gateway
    Api.Common
    Application.Common
    Test.Common
```

## Services

| Service              | Purpose                                                                                         |
|----------------------|-------------------------------------------------------------------------------------------------|
| `Main`               | Core business service: auth, users, products, storages, purchases, sales, balances, currencies. |
| `Search`             | Search API and local search indexes.                                                            |
| `Pricing`            | Pricing-related API and persistence.                                                            |
| `Analytics`          | Analytics API, metrics, and background worker.                                                  |
| `Gateway`            | Public entry point and reverse proxy.                                                           |
| `Api.Common`         | Shared API configuration and helpers.                                                           |
| `Application.Common` | Shared application contracts, validators, repository abstractions, and services.                |
| `Test.Common`        | Shared integration testing infrastructure, fixtures, stubs, and test contexts.                  |

## Infrastructure

The local Docker stack includes:

- PostgreSQL with `pg_cron`
- Redis Stack
- RabbitMQ with management UI
- MinIO
- Loki
- Prometheus
- Grafana

## Requirements

- .NET SDK 10
- Docker Desktop or Docker Engine
- Docker Compose
- Local TLS certificates for gateway/Grafana, or adjusted local configuration

Integration tests require Docker because they use Testcontainers.

## Configuration

Create a local `.env` from the example:

```bash
cp .env.example .env
```

The values in `.env.example` are development defaults. Do not use them in production and do not commit real secrets.

### Secrets

The repository contains `.env.example` only as a local development template. Some values look like passwords or signing
keys because Docker Compose needs complete defaults for a one-machine setup. Treat every value in `.env.example` as
disposable development data.

For real environments:

- keep secrets outside Git;
- use CI/CD variables, Docker secrets, Kubernetes secrets, Vault, cloud secret managers, or another environment-specific
  secret provider;
- rotate signing keys and service passwords per environment;
- do not reuse local MinIO, PostgreSQL, Redis, RabbitMQ, Grafana, JWT, or signing secrets;
- mount TLS certificates from infrastructure-managed storage, not from the repository.

Main secret/config groups:

| Group           | Variables                                                                                |
|-----------------|------------------------------------------------------------------------------------------|
| PostgreSQL      | `PGQL_USER`, `PGQL_PASSWORD`, `PGQL_MAIN_DB`, `PGQL_ANALYTICS_DB`, `PGQL_PRICING_DB`     |
| Redis           | `REDIS_PASSWORD`, `REDIS_HOST`, `REDIS_PORT`                                             |
| RabbitMQ        | `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`                                         |
| Gateway/JWT     | `GATEWAY_SUPER_KEY`, `VALID_ISSUER`, `ISSUER_SIGNING_KEY`                                |
| Service signing | `MAIN_SIGN_SECRET`, `PRICING_SIGN_SECRET`                                                |
| MinIO/S3        | `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD`, `MINIO_SERVICE_USER`, `MINIO_SERVICE_PASSWORD` |
| Grafana         | `GRAFANA_ADMIN_USER`, `GRAFANA_ADMIN_PASSWORD`                                           |
| TLS             | `CERTS_PATH`, `CERT_PATH`, `CERT_KEYPATH`                                                |

Important paths:

| Variable       | Description                                                    |
|----------------|----------------------------------------------------------------|
| `CONFIGS_PATH` | Mounted application config directory. Defaults to `./configs`. |
| `CERTS_PATH`   | Mounted certificate directory. Defaults to `./certs`.          |
| `CERT_PATH`    | Certificate path inside the container.                         |
| `CERT_KEYPATH` | Private key path inside the container.                         |

## Running Locally

Build and start the stack:

```bash
docker compose up -d --build
```

Gateway is exposed on:

```text
https://localhost:443
```

Useful local endpoints:

| Component     | Port    |
|---------------|---------|
| PostgreSQL    | `5432`  |
| RabbitMQ UI   | `15672` |
| MinIO Console | `9001`  |
| Loki          | `3100`  |
| Grafana       | `3000`  |

Stop services:

```bash
docker compose down
```

Stop services and remove local volumes:

```bash
docker compose down -v
```

## Database Migrations

Migrator containers are defined in `migrator-compose.yaml`.

Run all migrators:

```bash
docker compose -f migrator-compose.yaml up --build
```

Run a single migrator:

```bash
docker compose -f migrator-compose.yaml up --build main.migrator
docker compose -f migrator-compose.yaml up --build analytics.migrator
docker compose -f migrator-compose.yaml up --build pricing.migrator
```

The migrator compose file connects to PostgreSQL through `host.docker.internal:5432`, so PostgreSQL must be available
locally before running migrators.

## Build

Restore and build the solution:

```bash
dotnet restore ECommerceSpareParts.sln
dotnet build ECommerceSpareParts.sln
```

Build a specific project:

```bash
dotnet build src/Services/Main/Main.Tests/Main.Tests.csproj
```

## Tests

Run all tests:

```bash
dotnet test ECommerceSpareParts.sln
```

Run Main service tests:

```bash
dotnet test src/Services/Main/Main.Tests/Main.Tests.csproj
```

Run Analytics integration tests:

```bash
dotnet test src/Services/Analytics/Analytics.Integration.Tests/Analytics.Integration.Tests.csproj
```

Notes:

- Integration tests use Testcontainers.
- Docker must be running.
- Test projects may start PostgreSQL, Redis, or other dependency containers.

## Documentation

Development rules are kept outside the root README:

- [Development Guidelines](docs/DEVELOPMENT.md)
- [Testing Guidelines](docs/TESTING.md)
- [Main Tests README](src/Services/Main/Main.Tests/README.md)

## Observability

The compose stack includes:

- Loki for logs
- Prometheus for metrics
- Grafana for dashboards

Services receive `LOKI_URL` through environment variables. Grafana is configured to run over HTTPS using mounted
certificates.

## Storage

MinIO is used as an S3-compatible storage service. The `minio-init` container creates the configured images bucket and
service user during local startup.

Search service index data is mounted at:

```text
./search_api_data
```

## Common Commands

```bash
# Start full local stack
docker compose up -d --build

# Stop stack
docker compose down

# Stop stack and remove volumes
docker compose down -v

# Run all migrators
docker compose -f migrator-compose.yaml up --build

# Build solution
dotnet build ECommerceSpareParts.sln

# Run all tests
dotnet test ECommerceSpareParts.sln
```

## Production Notes

Before production deployment:

- Replace all development secrets.
- Use a real secret provider or environment-specific secret management.
- Configure TLS certificates outside the repository.
- Review Docker volume and backup strategy.
- Run migrations explicitly and verify database state.
- Enable CI checks for build and tests.
- Add health checks and deployment monitoring if they are not provided by the target platform.
