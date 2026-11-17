using System.Text.Json.Serialization;
using Main.Modules.Analytics.Abstractions.Interfaces.ChartData;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Attributes;

namespace Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;

public sealed record AbcXyzRunItemDataPoint : IChartDataPoint
{
	[JsonPropertyName("productId")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.product.id.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.product.id.description")]
	public required int ProductId { get; init; }

	[JsonPropertyName("rank")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.rank.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.rank.description")]
	public required int Rank { get; init; }

	[JsonPropertyName("value")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.value.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.value.description")]
	public required decimal Value { get; init; }

	[JsonPropertyName("share")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.share.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.share.description")]
	public required decimal Share { get; init; }

	[JsonPropertyName("cumulativeShare")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.cumulative.share.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.cumulative.share.description")]
	public required decimal CumulativeShare { get; init; }

	[JsonPropertyName("abc")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.abc.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.abc.description")]
	public required AbcClass Abc { get; init; }

	[JsonPropertyName("xyz")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.xyz.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.xyz.description")]
	public XyzClass? Xyz { get; init; }

	[JsonPropertyName("quantity")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.quantity.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.quantity.description")]
	public required decimal Quantity { get; init; }

	[JsonPropertyName("revenue")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.revenue.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.revenue.description")]
	public required decimal Revenue { get; init; }

	[JsonPropertyName("coefficientOfVariation")]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.coefficient.of.variation.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.coefficient.of.variation.description")]
	public decimal? CoefficientOfVariation { get; init; }

	[JsonPropertyName("periodsCount")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.abc.xyz.run.items.point.periods.count.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.point.periods.count.description")]
	public required int PeriodsCount { get; init; }
}
