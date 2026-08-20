using FluentAssertions;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Models;

namespace SchemaGeneration.Tests;

public sealed class SchemaLocalizerTests
{
    [Fact]
    public void Localize_ShouldLocalizeFieldsAndCsvColumnsWithoutChangingSourceSchema()
    {
        var source = CreateSchema();
        var localizer = new SchemaLocalizer(
            new StubScopedStringLocalizer(
                new Dictionary<string, string>
                {
                    ["field.label"] = "Field",
                    ["field.description"] = "Field description",
                    ["csv.label"] = "CSV column"
                }));

        var localized = localizer.Localize(source);

        var field = localized.Fields.Single();
        field.Label.Should().Be("Field");
        field.Description.Should().Be("Field description");
        field.LabelKey.Should().BeNull();
        field.DescriptionKey.Should().BeNull();

        var column = localized.CsvSchema!.Columns.Single();
        column.Label.Should().Be("CSV column");
        column.Description.Should().Be("missing.csv.description");
        column.LabelKey.Should().BeNull();
        column.DescriptionKey.Should().BeNull();

        source.Fields.Single().Label.Should().BeNull();
        source.Fields.Single().LabelKey.Should().Be("field.label");
    }

    [Fact]
    public void Localize_WhenSchemaIsNull_ShouldThrow()
    {
        var localizer = new SchemaLocalizer(
            new StubScopedStringLocalizer(new Dictionary<string, string>()));

        var action = () => localizer.Localize(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    private static ObjectSchema CreateSchema()
    {
        return new ObjectSchema
        {
            Fields =
            [
                new FieldSchema
                {
                    Name = "value",
                    Type = SchemaValueType.String,
                    LabelKey = "field.label",
                    DescriptionKey = "field.description"
                }
            ],
            CsvSchema = new CsvSchema
            {
                Columns =
                [
                    new CsvColumnSchema
                    {
                        PropertyName = "Value",
                        Names = ["Value"],
                        Type = SchemaValueType.String,
                        LabelKey = "csv.label",
                        DescriptionKey = "missing.csv.description"
                    }
                ]
            }
        };
    }
}
