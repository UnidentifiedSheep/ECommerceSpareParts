# Development Guidelines

This document describes project-level development rules for the solution.

## Architecture

- Keep domain behavior in entities and domain services.
- Keep use cases in application handlers and application services.
- Keep persistence details in persistence projects.
- Keep API endpoints thin: parse transport input, call application code, return API responses.
- Prefer existing shared abstractions before adding new ones.
- Avoid cross-service coupling through direct database access.
- Use contracts and integration events where service boundaries matter.

## Project Boundaries

Shared code belongs in shared projects only when at least two services need it.

Use service-specific projects for behavior that belongs to one bounded context:

- `*.Entities` for domain entities and domain exceptions.
- `*.Application` for handlers, validators, application services, projections, and consumers.
- `*.Persistence` for EF Core context, configurations, repositories, and persistence registration.
- `*.Api` for endpoints, API-specific setup, and host configuration.
- `*.Migrator` for database migrations and seed execution.

## Application Layer

- Use handlers for use cases.
- Keep handlers focused on orchestration.
- Keep reusable business operations in application services.
- Keep validation in validators and database validation components.
- Keep projection expressions reusable when they are used by queries.

## Domain Layer

- Domain entities should protect their invariants.
- Prefer domain methods over public setters.
- Throw domain-specific exceptions where the caller can handle a known business case.
- Keep infrastructure concerns out of domain entities.

## Persistence

- Keep EF Core mappings in configuration classes.
- Keep repository implementations in persistence projects.
- Avoid leaking EF-specific behavior into application handlers unless the project already uses an established pattern for it.
- Use transactions for multi-entity state changes that must be consistent.

## Configuration

- Do not commit real secrets.
- Keep development defaults in `.env.example`.
- Keep environment-specific values outside Git.
- Prefer explicit environment variables over hidden machine-specific setup.