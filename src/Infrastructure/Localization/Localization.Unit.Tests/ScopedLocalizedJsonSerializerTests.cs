using System.Text.Json;
using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
using CsvHelper.Configuration.Attributes;
using Enums;
using FluentAssertions;
using Localization.Domain;
using Localization.Domain.Serialization;

namespace Localization.Unit.Tests;

public class ScopedLocalizedJsonSerializerTests
{
    [Fact]
    public void Serialize_ShouldUseLocalizedPropertyName()
    {
        var serializer = CreateSerializer();
        var value = new TestInput
        {
            FileName = "producers.csv"
        };

        var json = serializer.Serialize(value);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        root.TryGetProperty("File", out var fileName).Should().BeTrue();
        fileName.GetString().Should().Be("producers.csv");
        root.TryGetProperty("fileName", out _).Should().BeFalse();
    }

    [Fact]
    public void SerializeMetadata_ShouldIncludeAttributesMetadata()
    {
        var serializer = CreateSerializer();

        var json = serializer.SerializeMetadata<TestInput>();

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement
            .GetProperty("fields")
            .EnumerateArray()
            .Single();

        field.GetProperty("name").GetString().Should().Be("fileName");
        field.GetProperty("type").GetString().Should().Be("string");
        field.GetProperty("label").GetString().Should().Be("File");
        field.GetProperty("description").GetString().Should().Be("CSV file with producers");
        field.GetProperty("required").GetBoolean().Should().BeTrue();
        field.GetProperty("control").GetString().Should().Be(nameof(InputControlType.UploadFile));
        field.GetProperty("accepts")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .BeEquivalentTo(".csv");
        field.GetProperty("dependsOnEntity").GetString().Should().Be(nameof(TestEntity));
        field.TryGetProperty("dependsOnField", out _).Should().BeFalse();
    }

    [Fact]
    public void SerializeMetadata_ShouldIncludeDependsOnField_WhenAttributeHasFieldName()
    {
        var serializer = CreateSerializer();

        var json = serializer.SerializeMetadata<TestInputWithEntityField>();

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement
            .GetProperty("fields")
            .EnumerateArray()
            .Single();

        field.GetProperty("dependsOnEntity").GetString().Should().Be("Product");
        field.GetProperty("dependsOnField").GetString().Should().Be("id");
    }

    [Fact]
    public void SerializeMetadata_ShouldIncludeCsvSchema_WhenTypeHasCsvSchemaAttribute()
    {
        var serializer = CreateSerializer();

        var json = serializer.SerializeMetadata<TestInputWithCsvSchema>();

        using var document = JsonDocument.Parse(json);
        var csvSchema = document.RootElement
            .GetProperty("csvSchema")
            .EnumerateArray()
            .ToList();

        csvSchema.Should().HaveCount(2);

        var sku = csvSchema.Single(x => x.GetProperty("propertyName").GetString() == nameof(TestCsvRow.Sku));
        sku.GetProperty("type").GetString().Should().Be("string");
        sku.GetProperty("required").GetBoolean().Should().BeTrue();
        sku.GetProperty("names")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .BeEquivalentTo(
                "Sku",
                "Article");

        var description = csvSchema.Single(x =>
            x.GetProperty("propertyName").GetString() == nameof(TestCsvRow.Description));
        description.GetProperty("required").GetBoolean().Should().BeFalse();
        description.GetProperty("label").GetString().Should().Be("Description");
        description.GetProperty("description").GetString().Should().Be("CSV row description");
    }

    [Fact]
    public void SerializeMetadata_ShouldFallbackToLocalizationKey_WhenLocalizedValueNotFound()
    {
        var serializer = CreateSerializer();

        var json = serializer.SerializeMetadata<TestInputWithMissingLocalization>();

        using var document = JsonDocument.Parse(json);
        var field = document.RootElement
            .GetProperty("fields")
            .EnumerateArray()
            .Single();

        field.GetProperty("label").GetString().Should().Be("missing_label_key");
        field.GetProperty("description").GetString().Should().Be("missing_description_key");
    }

    private static ScopedLocalizedJsonSerializer CreateSerializer()
    {
        var container = new LocalizerContainer("en");
        container.Initialize(
            new Dictionary<string, string>
            {
                ["file_name"] = "File",
                ["file_name_description"] = "CSV file with producers",
                ["csv_description_name"] = "Description",
                ["csv_description_description"] = "CSV row description"
            });

        var baseLocalizer = new StringLocalizer([container]);
        var scopedLocalizer = new ScopedStringLocalizer(baseLocalizer);
        scopedLocalizer.SetLocale("en");

        return new ScopedLocalizedJsonSerializer(scopedLocalizer);
    }

    private record TestInput
    {
        [JsonPropertyName("fileName")]
        [Accepts(".csv")]
        [InputControl(InputControlType.UploadFile)]
        [RequiredJsonField]
        [LocalizedJsonFieldDescription("file_name_description")]
        [LocalizedJsonFieldName("file_name")]
        [DependsOnEntity(typeof(TestEntity))]
        public required string FileName { get; init; }
    }

    private record TestInputWithMissingLocalization
    {
        [LocalizedJsonFieldName("missing_label_key")]
        [LocalizedJsonFieldDescription("missing_description_key")]
        public string? Value { get; init; }
    }

    private record TestInputWithEntityField
    {
        [DependsOnEntity("Product", "id")]
        public int ProductId { get; init; }
    }

    [CsvSchema(typeof(TestCsvRow))]
    private record TestInputWithCsvSchema
    {
        public string? FileName { get; init; }
    }

    private record TestCsvRow
    {
        [Name("Sku", "Article")]
        public required string Sku { get; init; }

        [Name("Description")]
        [Optional]
        [LocalizedJsonFieldName("csv_description_name")]
        [LocalizedJsonFieldDescription("csv_description_description")]
        public string? Description { get; init; }
    }

    private sealed class TestEntity;
}