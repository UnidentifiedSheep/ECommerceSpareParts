using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;
using SchemaGeneration.Abstractions.Exceptions;
using SchemaGeneration.Generators;

namespace SchemaGeneration.Tests;

public sealed class ReflectionSchemaGeneratorTests
{
	private readonly ReflectionSchemaGenerator _generator = new();

	[Fact]
	public void Generate_ShouldBuildFieldMetadata()
	{
		var schema = _generator.Generate<TestInput>();

		schema.Version.Should().Be(1);
		schema.Fields.Should().HaveCount(10);

		var file = schema.Fields.Single(x => x.Name == "fileName");
		file.Type.Should().Be(SchemaValueType.String);
		file.LabelKey.Should().Be("file.label");
		file.DescriptionKey.Should().Be("file.description");
		file.Label.Should().BeNull();
		file.Description.Should().BeNull();
		file.Required.Should().BeTrue();
		file.Control.Should().Be(InputControlType.UploadFile);
		file.Accepts.Should().Equal(".csv", "text/csv");
		file.Dependency.Should().NotBeNull();
		file.Dependency!.EntityName.Should().Be(nameof(TestEntity));
		file.Dependency.FieldName.Should().Be("id");
	}

	[Fact]
	public void Generate_ShouldMapSupportedPropertyTypes()
	{
		var schema = _generator.Generate<TestInput>();

		schema.Fields.Single(x => x.Name == "identifier").Type.Should().Be(SchemaValueType.String);
		schema.Fields.Single(x => x.Name == "createdAt").Type.Should().Be(SchemaValueType.String);
		schema.Fields.Single(x => x.Name == "enabled").Type.Should().Be(SchemaValueType.Boolean);
		schema.Fields.Single(x => x.Name == "count").Type.Should().Be(SchemaValueType.Integer);
		schema.Fields.Single(x => x.Name == "amount").Type.Should().Be(SchemaValueType.Number);
		schema.Fields.Single(x => x.Name == "state").Type.Should().Be(SchemaValueType.Enum);
		schema.Fields.Single(x => x.Name == "items").Type.Should().Be(SchemaValueType.Array);
		schema.Fields.Single(x => x.Name == "nested").Type.Should().Be(SchemaValueType.Object);
		schema.Fields.Single(x => x.Name == "optionalCount").Type.Should().Be(SchemaValueType.Integer);
	}

	[Fact]
	public void Generate_ShouldBuildCsvSchema()
	{
		var schema = _generator.Generate<CsvInput>();

		schema.CsvSchema.Should().NotBeNull();
		schema.CsvSchema!.Columns.Should().HaveCount(2);

		var sku = schema.CsvSchema.Columns.Single(x => x.PropertyName == nameof(CsvRow.Sku));
		sku.Names.Should().Equal("Sku", "Article");
		sku.Type.Should().Be(SchemaValueType.String);
		sku.Required.Should().BeTrue();

		var description = schema.CsvSchema.Columns.Single(x => x.PropertyName == nameof(CsvRow.Description));
		description.Names.Should().Equal(nameof(CsvRow.Description));
		description.Required.Should().BeFalse();
		description.LabelKey.Should().Be("csv.description.label");
		description.DescriptionKey.Should().Be("csv.description.description");
	}

	[Fact]
	public void Generate_WhenTypeHasNoCsvAttribute_ShouldNotBuildCsvSchema()
	{
		var schema = _generator.Generate<TestInput>();

		schema.CsvSchema.Should().BeNull();
	}

	[Fact]
	public void Generate_WhenRootTypeIsNotObject_ShouldThrow()
	{
		var action = () => _generator.Generate(typeof(string));

		action.Should().Throw<SchemaGenerationException>().Which.SchemaType.Should().Be(typeof(string));
	}

	[Fact]
	public void Generate_WithRuntimeType_ShouldReturnSameCachedSchemaAsGenericOverload()
	{
		var genericSchema = _generator.Generate<TestInput>();

		var runtimeSchema = _generator.Generate(typeof(TestInput));

		runtimeSchema.Should().BeSameAs(genericSchema);
	}

	[Fact]
	public void Generate_WhenRuntimeTypeIsNull_ShouldThrow()
	{
		var action = () => _generator.Generate(null!);

		action.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Generate_ShouldIncludeInheritedPropertiesAndExcludeJsonIgnoredProperties()
	{
		var schema = _generator.Generate<DerivedInput>();

		schema
		.Fields
		.Select(x => x.Name)
		.Should()
		.BeEquivalentTo(
			"baseValue",
			"alwaysIncluded",
			"derivedValue");
	}

	[Fact]
	public void Generate_ShouldBuildDependencyFromEntityNameWithoutFieldName()
	{
		var schema = _generator.Generate<NamedDependencyInput>();

		schema
			.Fields
			.Single()
			.Dependency
			.Should()
			.BeEquivalentTo(
				new
				{
					EntityName = "catalogue-product", FieldName = (string?)null
				});
	}

	[Fact]
	public async Task Generate_WhenCalledConcurrently_ShouldReturnSingleCachedSchema()
	{
		var schemas = await Task.WhenAll(
			Enumerable.Range(0, 100).Select(_ => Task.Run(() => _generator.Generate<ConcurrentInput>())));

		schemas.Should().OnlyContain(schema => ReferenceEquals(schema, schemas[0]));
	}

	private sealed record TestInput
	{
		[JsonPropertyName("fileName")]
		[SchemaFieldLabel("file.label")]
		[SchemaFieldDescription("file.description")]
		[RequiredSchemaField]
		[SchemaInputControl(InputControlType.UploadFile)]
		[SchemaAccepts(".csv", "text/csv")]
		[SchemaDependsOnEntity(typeof(TestEntity), "id")]
		public required string File { get; init; }

		public Guid Identifier { get; init; }

		public DateTime CreatedAt { get; init; }

		public bool Enabled { get; init; }

		public int Count { get; init; }

		public decimal Amount { get; init; }

		public TestState State { get; init; }

		public int[] Items { get; init; } = [];

		public TestNested? Nested { get; init; }

		public int? OptionalCount { get; init; }
	}

	[CsvSchema(typeof(CsvRow))]
	private sealed record CsvInput;

	private sealed record CsvRow
	{
		[Name("Sku", "Article")]
		public required string Sku { get; init; }

		[Optional]
		[SchemaFieldLabel("csv.description.label")]
		[SchemaFieldDescription("csv.description.description")]
		public string? Description { get; init; }
	}

	private sealed record TestNested;

	private sealed class TestEntity;

	private abstract record BaseInput
	{
		[JsonPropertyName("baseValue")]
		public string? BaseValue { get; init; }

		[JsonIgnore]
		public string? IgnoredValue { get; init; }

		[JsonPropertyName("alwaysIncluded")]
		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public string? AlwaysIncluded { get; init; }
	}

	private sealed record DerivedInput : BaseInput
	{
		[JsonPropertyName("derivedValue")]
		public string? DerivedValue { get; init; }
	}

	private sealed record NamedDependencyInput
	{
		[SchemaDependsOnEntity("catalogue-product")]
		public Guid ProductId { get; init; }
	}

	private sealed record ConcurrentInput
	{
		public string? Value { get; init; }
	}

	private enum TestState
	{
		Unknown,

		Active
	}
}
