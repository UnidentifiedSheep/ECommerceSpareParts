using System.Text.Json.Serialization;
using Main.Modules.Analytics.Abstractions.Interfaces.ChartData;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRunItems;

public sealed record CoPurchaseRunItemDataPoint : IChartDataPoint
{
	private const string ProductIdField = "productId";

	private const string TopRelatedProductIdField = "topRelatedProductId";

	[JsonPropertyName(ProductIdField)]
	[RequiredSchemaField]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.product.id.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.product.id.description")]
	public required int ProductId { get; init; }

	[JsonPropertyName("salesCount")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.sales.count.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.sales.count.description")]
	public required int SalesCount { get; init; }

	[JsonPropertyName("relatedCount")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.related.count.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.related.count.description")]
	public required int RelatedCount { get; init; }

	[JsonPropertyName(TopRelatedProductIdField)]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.top.related.product.id.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.top.related.product.id.description")]
	public int? TopRelatedProductId { get; init; }

	[JsonPropertyName("topConfidence")]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.top.confidence.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.top.confidence.description")]
	public decimal? TopConfidence { get; init; }

	[JsonPropertyName("topLift")]
	[SchemaFieldLabel("chart.co.purchase.run.items.point.top.lift.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.point.top.lift.description")]
	public decimal? TopLift { get; init; }

	public IReadOnlyList<ChartReference> GetReferences()
		=>
		[
			new(SchemaEntity.Product, ProductIdField, ProductId),
			new(SchemaEntity.Product, TopRelatedProductIdField, TopRelatedProductId)
		];
}
