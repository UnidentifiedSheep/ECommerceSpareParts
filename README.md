# ECommerceSpareParts

.NET backend for a spare-parts platform: catalog, warehouse operations, supplier offers, pricing, search, sales, and
analytics.

> The project is under active development. See the [roadmap](docs/TODO.md) for incomplete features.

## Services

| Service | Responsibility |
| --- | --- |
| `Gateway` | Public YARP reverse proxy and aggregated API documentation. |
| `Main` | Catalog, users, WMS, purchases, sales, logistics, and files. |
| `Pricing` | Supplier/internal offers, markup rules, price generation, and ranking. |
| `Search` | Product and producer search backed by OpenSearch. |
| `Analytics` | Metrics, sale analysis, and automatic markup generation. |

Main, Pricing, and Analytics have separate API, worker, persistence, test, and migrator projects where needed. Services
exchange integration events through RabbitMQ and use the EF Core outbox pattern.

## Core Features

- [Main service](docs/MAIN.md) — catalog, users, commercial operations, finances, currencies, imports, and files.
- [Pricing and markup](docs/PRICING.md) — offer collection, price rules, automatic markups, and recalculation.
- [Warehouse management](docs/WMS.md) — stock lots, purchases, sales, reservations, and logistics.
- [Search service](docs/SEARCH.md) — product/SKU search, filters, producers, aliases, and index synchronization.
- [Analytics service](docs/ANALYTICS.md) — purchase/sale facts, product metrics, and markup analysis.
- Favorit supplier integration and Armtek client infrastructure.
- Background and scheduled jobs with progress updates through SignalR.

## Stack

.NET 10, ASP.NET Core Minimal APIs, EF Core, PostgreSQL, RabbitMQ/MassTransit, Redis, OpenSearch, MinIO, YARP,
OpenTelemetry, Prometheus, Loki, Grafana, xUnit, Testcontainers, and Docker Compose.

## Quick Start

Requirements: .NET 10 SDK, Docker, and Docker Compose v2.

```bash
cp .env.example .env

# Start dependencies
docker compose up -d pgql redis rabbitmq opensearch minio minio-init

# Apply migrations and seed data
docker compose -f migrator-compose.yaml up --build --abort-on-container-exit

# Start the complete stack
docker compose up -d --build
```

Open the API documentation at <http://localhost:8080/docs>.

Grafana uses HTTPS and requires `CERT_PATH` and `CERT_KEYPATH` pointing to certificate files mounted from `CERTS_PATH`.

Stop the stack with `docker compose down`. Add `-v` to remove development volumes as well.

## Local Endpoints

| Component | Address |
| --- | --- |
| Gateway | <http://localhost:8080> |
| API documentation | <http://localhost:8080/docs> |
| RabbitMQ UI | <http://localhost:15672> |
| OpenSearch | <https://localhost:9200> |
| OpenSearch Dashboards | <http://localhost:5601> |
| MinIO Console | <http://localhost:9001> |
| Grafana | <https://localhost:3000> |

PostgreSQL, Redis, RabbitMQ, MinIO, and Loki are exposed on their standard development ports. Prometheus receives a
dynamic host port; find it with `docker compose port prometheus 9090`.

## Build and Test

```bash
dotnet restore ECommerceSpareParts.sln
dotnet build ECommerceSpareParts.sln --no-restore
dotnet test ECommerceSpareParts.sln --no-build
```

Integration tests use Testcontainers and require Docker.

## Configuration

Copy `.env.example` to `.env`. Application settings are loaded from `configs/`, which is mounted into service
containers through `CONFIGS_PATH`.

Values in `.env.example` are development-only. Use an external secret provider and managed TLS certificates in deployed
environments.

## Compose Files

- `compose.yaml` — local stack built from source;
- `migrator-compose.yaml` — local database migrators;
- `compose.registry.yaml` — registry image builds;
- [`deploy/`](docs/SWARM_DEPLOYMENT.md) — split Docker Swarm stacks for secrets sync, gateway, applications, workers, infrastructure, and monitoring.

## Documentation

- [Main service](docs/MAIN.md)
- [Pricing and markup](docs/PRICING.md)
- [Warehouse management](docs/WMS.md)
- [Search service](docs/SEARCH.md)
- [Analytics service](docs/ANALYTICS.md)
- [Development guidelines](docs/DEVELOPMENT.md)
- [Testing guidelines](docs/TESTING.md)
- [Docker Swarm deployment](docs/SWARM_DEPLOYMENT.md)
- [PostgreSQL backup and recovery](docs/POSTGRES_BACKUP.md)
- [Roadmap](docs/TODO.md)

## License

See [LICENSE](LICENSE).
