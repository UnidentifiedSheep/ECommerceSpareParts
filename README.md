# ECommerceSpareParts

Репозиторий содержит распределённое .NET-решение для электронной коммерции запчастей (modular microservices-style). Включает несколько сервисов (API, Gateway, Analytics), инфраструктурные библиотеки (Redis, RabbitMQ, Persistence, Mail, Integrations и т.д.) и общие библиотеки ядра/контрактов.

## Краткое содержание
- **Ядро**: `src/Core` — доменные модели и контракты
- **Инфраструктура**: `src/Infrastructure` — адаптеры для БД, Redis, RabbitMQ, Mail, S3-интеграции и пр.
- **Сервисы**: `src/Services` — набор отдельных сервисов: Main (Api / Application / Persistence / Core), Analytics, Gateway и пр.
- **compose.yaml** — готовая конфигурация для локального запуска через Docker Compose (Postgres, Redis, RabbitMQ и сервисы).
- **certs/** — самоподписанные сертификаты (используются в compose.yaml для Kestrel)

## Технологический стек
- **Язык**: C# (.NET 9 / net9.0)
- **Веб**: ASP.NET Core
- **ORM**: Entity Framework Core (Npgsql для PostgreSQL)
- **Очереди**: RabbitMQ (+ MassTransit)
- **Кэш**: Redis (StackExchange.Redis)
- **Reverse proxy**: YARP (в Gateway)
- **Аутентификация**: JWT
- **Интеграции**: AWS S3 (AWSSDK.S3), MailKit
- **Observability**: OpenTelemetry, Prometheus, Serilog (Loki sink)
- **Задачи/фон**: Hangfire (Postgres storage)
- **API docs**: Swashbuckle / Swagger
- **Прочее**: Carter (микро-рутер/функциональные маршруты)

## Требования (для разработки)
- .NET SDK 9.0 (dotnet 9)
- Docker Desktop (если запускаете через Docker Compose)
- Git
- Рекомендуется IDE: JetBrains Rider или Visual Studio 2022/2023 с поддержкой .NET 9

## Быстрый старт (Docker Compose)
В корне репозитория находится `compose.yaml` — он поднимает сервисы API, Gateway и зависимости (Postgres, Redis, RabbitMQ).

Откройте командную строку (cmd.exe) и выполните:

```cmd
dotnet --version
docker compose -f compose.yaml up --build
```

После старта сервисы будут доступны по портам (см. маппинг в `compose.yaml`):
- **main.api**: https=57005:7292 и http=57015:8080
- **analytics.api**: https=57004:7292 и http=57014:8080
- **gateway**: https=57006:8081
- **PostgreSQL**: 5432:5432
- **Redis**: 6379:6379
- **RabbitMQ**: 5672:5672 (AMQP), 15672:15672 (management UI)

### Пароли / сертификаты
- В compose.yaml для dev указаны credentials и пароль сертификата. Это только для локальной разработки и тестов.
  - **Postgres**: `POSTGRES_PASSWORD=PleasKillMe21`
  - **PFX (certs/localhost.pfx)** пароль: `PleasKillMe.21`
- Сертификаты находятся в каталоге `certs/` и смонтированы в контейнеры. Не используйте эти секреты в production.

Примечание: для корректной работы `dotnet ef` может понадобиться установить global tool `dotnet-ef` и убедиться, что в `Persistence` проекте присутствует DbContext с корректным конструктором, использующим IConfiguration/DI.

## Архитектура и структура папок (основное)
- **src/Core** — контракты, модели и общая бизнес-логика
  - Contracts
  - Core
  - Exceptions
- **src/Infrastructure** — адаптеры и интеграции
  - Common, Mail, Persistence, RabbitMq, Redis, Integrations, Security и т.д.
- **src/Services** — сервисы верхнего уровня
  - Main (Api / Application / Persistence / Core / Tests)
  - Analytics (Api / Application / Persistence / Core)
  - Gateway (YARP reverse proxy)
  - Gateway и Main.Api содержат Dockerfile (используются в compose.yaml)

## Полезные подсказки для разработчиков
- Gateway использует YARP и JWT-аутентификацию. Конфигурация proxy и auth находится в соответствующих проектах/файлах конфигурации.
- Observability: API проэкты подключают OpenTelemetry и prometheus-net, проверьте endpointы `/metrics` и `/otel` если нужно интегрировать.
- Для отправки писем используется MailKit (Infrastructure/Mail).
- Интеграция с S3 реализована в Infrastructure/Integrations (AWSSDK.S3). Убедитесь, что прокси/ключи AWS настроены через переменные окружения или секреты.

## Запуск в среде разработки
- Для разработки удобно запускать PostgreSQL/Redis/RabbitMQ локально в Docker, а сервисы — локально из IDE. Укажите переменные окружения (connection strings, JWT secret и т.п.) либо через пользовательские secrets/переменные окружения в IDE.

## CI / тесты
- В репозитории есть тестовый проект `src/Services/Main/Main.Tests`. Запуск: `dotnet test`.