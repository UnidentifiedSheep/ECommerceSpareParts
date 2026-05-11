# Testing Guidelines

This document describes the solution-level testing approach.

## Test Types

- Domain tests verify entity behavior and invariants.
- Handler tests verify application use cases through `Mediator.Send(...)`.
- Integration tests verify database, infrastructure, and application wiring.

Integration tests use Testcontainers, so Docker must be running.

## Test Data Pattern

Use this split consistently:

- `DataBuilders` create valid domain entities.
- `TestContexts` prepare reusable database state.
- Handler tests execute application behavior through `Mediator.Send(...)`.

## Data Builders

Builder rules:

- Builders should construct entities only.
- Builders should not use `DbContext`.
- Builders should not use `Mediator`.
- Builders should expose `With...` methods for fields that tests commonly override.
- Builders should create valid defaults so each test only specifies data relevant to the case.

Use shared builder extensions when possible:

- `BuildAndAddToDb`
- `BuildManyAndAddToDb`
- `BuildManyCombinedAndAddToDb`

Manual `DbContext.Add...` is acceptable when an entity must be built, connected to other entities, mutated through domain methods, and saved as one consistent setup.

## Test Contexts

Context rules:

- Contexts prepare reusable data through builders, domain methods, and `DbContext`.
- Contexts must not call handlers through `Mediator.Send(...)` to create seed data.
- Contexts should keep setup small and predictable.
- Contexts should expose created entities through read-only properties.
- Contexts should declare dependencies with `IDependentTestContext.DependsOn`.

Register feature-specific contexts in the test constructor:

```csharp
public EditProductTests(CombinedContainerFixture fixture) : base(fixture)
{
    RegisterBasicContext<ProductTestContext>();
}

private ProductTestContext TestContext => GetContext<ProductTestContext>();
```

Use global test contexts only for setup needed by most tests.

## Handler Tests

Handler test rules:

- Use `Mediator.Send(...)` for the behavior under test.
- Read setup data from registered test contexts.
- Do not create common seed data inside every test case when a context already exists.
- Validate database side effects through `Context` or the context's `DbContext`.