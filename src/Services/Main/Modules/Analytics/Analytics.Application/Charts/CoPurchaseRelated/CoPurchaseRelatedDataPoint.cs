using System.Text.Json.Serialization;
using Main.Modules.Analytics.Abstractions.Interfaces.ChartData;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRelated;

public sealed record CoPurchaseRelatedDataPoint : IChartDataPoint
{
	private const string ProductIdField = "productId";

	private const string SourceProductIdField = "sourceProductId";

	[JsonPropertyName("position")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.related.point.position.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.position.description")]
	public required int Position { get; init; }

	[JsonPropertyName(ProductIdField)]
	[RequiredSchemaField]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel("chart.co.purchase.related.point.product.id.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.product.id.description")]
	public required int ProductId { get; init; }

	[JsonPropertyName("variantProductIds")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.related.point.variant.product.ids.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.variant.product.ids.description")]
	public required IReadOnlyList<int> VariantProductIds { get; init; }

	[JsonPropertyName("pairCount")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.related.point.pair.count.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.pair.count.description")]
	public required int PairCount { get; init; }

	[JsonPropertyName("confidence")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.related.point.confidence.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.confidence.description")]
	public required decimal Confidence { get; init; }

	[JsonPropertyName("lift")]
	[RequiredSchemaField]
	[SchemaFieldLabel("chart.co.purchase.related.point.lift.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.lift.description")]
	public required decimal Lift { get; init; }

	[JsonPropertyName(SourceProductIdField)]
	[RequiredSchemaField]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel("chart.co.purchase.related.point.source.product.id.name")]
	[SchemaFieldDescription("chart.co.purchase.related.point.source.product.id.description")]
	public required int SourceProductId { get; init; }

	public IReadOnlyList<ChartReference> GetReferences()
		=>
		[
			new(SchemaEntity.Product, ProductIdField, ProductId),
			new(SchemaEntity.Product, SourceProductIdField, SourceProductId)
		];
}
