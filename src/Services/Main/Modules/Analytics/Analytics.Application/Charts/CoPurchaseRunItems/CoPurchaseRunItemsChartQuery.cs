using System.Text.Json.Serialization;
using Domain.Validation;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Charts;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.CoPurchaseRunItems;

public sealed record CoPurchaseRunItemsChartQuery : CursorChartQueryInput<int>
{
	[JsonPropertyName("runId")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("chart.co.purchase.run.items.query.run.id.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.query.run.id.description")]
	public required Guid RunId { get; init; }

	[JsonPropertyName("hasRelated")]
	[SchemaFieldLabel("chart.co.purchase.run.items.query.has.related.name")]
	[SchemaFieldDescription("chart.co.purchase.run.items.query.has.related.description")]
	public bool? HasRelated { get; init; }

	public void Validate()
	{
		RunId.Ensure(id => id != Guid.Empty, ChartCoPurchaseRunItemsQueryRunIdInvalidMessage.Instance);
		if (Cursor is { } cursor)
			cursor.Ensure(value => value >= 0, ChartCoPurchaseRunItemsQueryCursorInvalidMessage.Instance);
		ValidateCursor();
	}
}
