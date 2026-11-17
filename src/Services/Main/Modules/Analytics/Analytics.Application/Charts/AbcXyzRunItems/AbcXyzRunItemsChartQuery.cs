using System.Text.Json.Serialization;
using Domain.Validation;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Abstractions.Models;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.AbcXyzRunItems;

public sealed record AbcXyzRunItemsChartQuery : CursorChartQueryInput<int>
{
	[JsonPropertyName("runId")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("chart.abc.xyz.run.items.query.run.id.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.query.run.id.description")]
	public required Guid RunId { get; init; }

	[JsonPropertyName("abc")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(nameof(AbcClass))]
	[SchemaFieldLabel("chart.abc.xyz.run.items.query.abc.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.query.abc.description")]
	public AbcClass? Abc { get; init; }

	[JsonPropertyName("xyz")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(nameof(AbcXyzRunItemsXyzFilter))]
	[SchemaFieldLabel("chart.abc.xyz.run.items.query.xyz.name")]
	[SchemaFieldDescription("chart.abc.xyz.run.items.query.xyz.description")]
	public AbcXyzRunItemsXyzFilter? Xyz { get; init; }

	public void Validate()
	{
		RunId.Ensure(id => id != Guid.Empty, ChartAbcXyzRunItemsQueryRunIdInvalidMessage.Instance);

		if (Abc is { } abc)
			abc.Ensure(Enum.IsDefined, ChartAbcXyzRunItemsQueryAbcUnsupportedMessage.Instance);

		if (Xyz is { } xyz)
			xyz.Ensure(Enum.IsDefined, ChartAbcXyzRunItemsQueryXyzUnsupportedMessage.Instance);

		if (Cursor is { } cursor)
			cursor.Ensure(value => value >= 0, ChartAbcXyzRunItemsQueryCursorInvalidMessage.Instance);

		ValidateCursor();
	}
}
