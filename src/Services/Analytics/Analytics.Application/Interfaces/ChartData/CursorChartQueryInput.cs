using System.Text.Json.Serialization;
using Abstractions.Models;
using Domain.Validation;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Analytics.Application.Interfaces.ChartData;

public abstract record CursorChartQueryInput<TCursor> : ICursorChartQueryInput<TCursor> where TCursor : struct
{
	[JsonPropertyName("cursor")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pagination.cursor.name")]
	[SchemaFieldDescription("pagination.cursor.description")]
	public TCursor? Cursor { get; init; }

	[JsonPropertyName("size")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("pagination.size.name")]
	[SchemaFieldDescription("pagination.size.description")]
	public int Size { get; init; } = 100;

	public Cursor<TCursor?> GetCursor() => new(Cursor, Size);

	protected void ValidateCursor()
	{
		Size.EnsureInRange(
			1,
			100,
			"pagination.size.range");
	}
}
