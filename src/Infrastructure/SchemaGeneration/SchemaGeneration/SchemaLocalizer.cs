using Localization.Abstractions.Interfaces;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration;

public sealed class SchemaLocalizer(
    IScopedStringLocalizer localizer
) : ISchemaLocalizer
{
    public ObjectSchema Localize(ObjectSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return schema with
        {
            Fields = schema.Fields.Select(Localize).ToArray(),
            CsvSchema = schema.CsvSchema is null
                ? null
                : new CsvSchema
                {
                    Columns = schema.CsvSchema.Columns.Select(Localize).ToList()
                }
        };
    }

    private FieldSchema Localize(FieldSchema field)
    {
        return field with
        {
            Label = GetLocalizedOrDefault(field.LabelKey),
            Description = GetLocalizedOrDefault(field.DescriptionKey),
            LabelKey = null,
            DescriptionKey = null
        };
    }

    private CsvColumnSchema Localize(CsvColumnSchema column)
    {
        return column with
        {
            Label = GetLocalizedOrDefault(column.LabelKey),
            Description = GetLocalizedOrDefault(column.DescriptionKey),
            LabelKey = null,
            DescriptionKey = null
        };
    }

    private string? GetLocalizedOrDefault(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var value = localizer.GetOrDefault(key);
        return string.IsNullOrWhiteSpace(value) ? key : value;
    }
}
