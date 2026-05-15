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

Use a `TestContext` when the setup:

- is reused by more than one test,
- represents a valid reusable aggregate or related seed graph,
- or has its own dependency graph via `DependsOn`.

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
- Contexts compose builders and persist their results; entity construction logic belongs in `DataBuilders`.
- Contexts must not call handlers through `Mediator.Send(...)` to create seed data.
- Contexts should keep setup small and predictable.
- Contexts should expose created entities through read-only properties.
- Contexts should declare dependencies with `IDependentTestContext.DependsOn`.
- If multiple tests need the same valid aggregate or related seed graph, create a dedicated `TestContext`
  instead of rebuilding it inside each test.
- Prefer shared builder extensions such as `BuildAndAddToDb`, `BuildManyAndAddToDb`, and
  `BuildManyCombinedAndAddToDb` from contexts instead of manually calling `Build()`, `AddRangeAsync()`,
  and `SaveChangesAsync()` when the setup fits those helpers.

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
- Do not use one handler to prepare state for another handler test. If setup is reusable, use a `TestContext`;
  if it is scenario-specific, build the required entities through `DataBuilders` and domain methods.
- Validate database side effects through `Context` or the context's `DbContext`.
