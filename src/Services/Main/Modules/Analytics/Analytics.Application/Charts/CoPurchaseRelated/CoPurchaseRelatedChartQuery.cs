using System.Text.Json.Serialization;
using Domain.Validation;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Interfaces.ChartData;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRelated;

public sealed record CoPurchaseRelatedChartQuery : IChartQueryInput
{
	[JsonPropertyName("productId")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Product, "id")]
	[SchemaFieldLabel("chart.co.purchase.related.query.product.id.name")]
	[SchemaFieldDescription("chart.co.purchase.related.query.product.id.description")]
	public required int ProductId { get; init; }

	[JsonPropertyName("runId")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("chart.co.purchase.related.query.run.id.name")]
	[SchemaFieldDescription("chart.co.purchase.related.query.run.id.description")]
	public Guid? RunId { get; init; }

	public void Validate()
	{
		ProductId.Ensure(id => id > 0, ChartCoPurchaseRelatedQueryProductIdInvalidMessage.Instance);
		if (RunId is { } runId)
			runId.Ensure(id => id != Guid.Empty, ChartCoPurchaseRelatedQueryRunIdInvalidMessage.Instance);
	}
}
