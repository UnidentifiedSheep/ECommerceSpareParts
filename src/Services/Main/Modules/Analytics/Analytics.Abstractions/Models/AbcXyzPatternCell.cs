using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzPatternCell(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.pattern.cell.abc.name")]
	[property: SchemaFieldDescription("abc.xyz.pattern.cell.abc.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AbcClass Abc,
	[property: SchemaFieldLabel("abc.xyz.pattern.cell.pattern.name")]
	[property: SchemaFieldDescription("abc.xyz.pattern.cell.pattern.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	DemandPattern? Pattern,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.pattern.cell.products.count.name")]
	[property: SchemaFieldDescription("abc.xyz.pattern.cell.products.count.description")]
	int ProductsCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.pattern.cell.value.name")]
	[property: SchemaFieldDescription("abc.xyz.pattern.cell.value.description")]
	decimal Value);
