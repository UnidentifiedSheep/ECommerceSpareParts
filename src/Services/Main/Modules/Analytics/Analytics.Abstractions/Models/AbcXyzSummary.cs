using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzSummary(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.summary.products.count.name")]
	[property: SchemaFieldDescription("abc.xyz.summary.products.count.description")]
	int ProductsCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.summary.periods.count.name")]
	[property: SchemaFieldDescription("abc.xyz.summary.periods.count.description")]
	int PeriodsCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.summary.total.value.name")]
	[property: SchemaFieldDescription("abc.xyz.summary.total.value.description")]
	decimal TotalValue,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.summary.cells.name")]
	[property: SchemaFieldDescription("abc.xyz.summary.cells.description")]
	IReadOnlyList<AbcXyzCell> Cells);

public sealed record AbcXyzCell(
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.cell.abc.name")]
	[property: SchemaFieldDescription("abc.xyz.cell.abc.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AbcClass Abc,
	[property: SchemaFieldLabel("abc.xyz.cell.xyz.name")]
	[property: SchemaFieldDescription("abc.xyz.cell.xyz.description")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	XyzClass? Xyz,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.cell.products.count.name")]
	[property: SchemaFieldDescription("abc.xyz.cell.products.count.description")]
	int ProductsCount,
	[property: RequiredSchemaField]
	[property: SchemaFieldLabel("abc.xyz.cell.value.name")]
	[property: SchemaFieldDescription("abc.xyz.cell.value.description")]
	decimal Value);
