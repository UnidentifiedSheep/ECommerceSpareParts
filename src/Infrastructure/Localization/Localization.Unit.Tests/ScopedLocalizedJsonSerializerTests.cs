using System.Text.Json;
using System.Text.Json.Serialization;
using Attributes.JsonAttributes;
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
        field.GetProperty("accepts").EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .BeEquivalentTo([".csv"]);
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
        container.Initialize(new Dictionary<string, string>
        {
            ["file_name"] = "File",
            ["file_name_description"] = "CSV file with producers"
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
        public required string FileName { get; init; }
    }

    private record TestInputWithMissingLocalization
    {
        [LocalizedJsonFieldName("missing_label_key")]
        [LocalizedJsonFieldDescription("missing_description_key")]
        public string? Value { get; init; }
    }
}
