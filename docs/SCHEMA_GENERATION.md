# Schema Generation

Schema Generation builds metadata for JSON input models and optional CSV files. Clients use this metadata to render
forms for settings and long-running jobs.

## Setup

Projects that only declare schema attributes should reference:

```xml
<ProjectReference Include="path/to/SchemaGeneration.Abstractions.csproj" />
```

Register the implementation in the composition root:

```csharp
services.AddSchemaGeneration();
```

Services using `Application.Common.AddApplicationBase` already have this registration.

## Define a Schema

Add schema attributes to the input model:

```csharp
using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

public sealed record ImportInput
{
    [JsonPropertyName("fileName")]
    [RequiredSchemaField]
    [SchemaInputControl(InputControlType.UploadFile)]
    [SchemaAccepts(".csv", "text/csv")]
    [SchemaFieldLabel("file_name")]
    [SchemaFieldDescription("file_name_description")]
    public required string FileName { get; init; }

    [JsonPropertyName("productId")]
    [SchemaInputControl(InputControlType.EntitySelector)]
    [SchemaDependsOnEntity("Product", "id")]
    public int ProductId { get; init; }
}
```

| Attribute | Purpose |
| --- | --- |
| `RequiredSchemaField` | Marks the field as required in metadata. |
| `SchemaInputControl` | Suggests a UI control. |
| `SchemaAccepts` | Defines accepted extensions or MIME types. |
| `SchemaDependsOnEntity` | Defines the entity used by a selector. |
| `SchemaFieldLabel` | Defines the localization key for the label. |
| `SchemaFieldDescription` | Defines the localization key for the description. |
| `CsvSchema` | Associates the input with a CSV row type. |

## Generate a Schema

Inject the default generator into an application handler:

```csharp
public sealed class Handler(ISchemaGenerator schemaGenerator)
{
    public ObjectSchema GetSchema(Type inputType)
    {
        return schemaGenerator.Generate(inputType);
    }
}
```

The default `ISchemaGenerator` is scoped and returns a schema localized for the current request.

Use the singleton raw generator when localization is not needed:

```csharp
var generator = serviceProvider.GetRequiredKeyedService<ISchemaGenerator>(
    SchemaGeneratorKind.Raw);
```

The raw generator returns `LabelKey` and `DescriptionKey`. The localized generator replaces them with `Label` and
`Description`. A missing translation falls back to the localization key.

## CSV Metadata

Use CsvHelper attributes on the CSV row model:

```csharp
using CsvHelper.Configuration.Attributes;

[CsvSchema(typeof(ImportCsvRow))]
public sealed record ImportInput;

public sealed record ImportCsvRow
{
    [Name("Sku", "Article")]
    public required string Sku { get; init; }

    [Optional]
    public string? Description { get; init; }
}
```

Columns are required by default. `[Optional]` makes a column optional, and `[Name]` defines its accepted aliases.

## Important Rules

- Return `ObjectSchema` directly from API DTOs. Do not serialize it into a nested JSON string.
- Localization changes only `Label` and `Description`. It never changes JSON property names or application data.
- Use `[JsonPropertyName]` when the schema field name must differ from the CLR property name.
- `[JsonIgnore]` with `JsonIgnoreCondition.Always` excludes a property from the schema.
- The root schema type must be a JSON object. Primitive root types throw `SchemaGenerationException`.
- `RequiredSchemaField` is UI metadata and does not replace runtime validation.
- Raw schemas are cached by `Type`. Do not put request-specific or locale-specific data into them.
- Increment `SchemaContractVersion.Current` when the serialized schema contract changes incompatibly.

## Tests

```bash
dotnet test src/Infrastructure/SchemaGeneration/SchemaGeneration.Tests/SchemaGeneration.Tests.csproj
```
