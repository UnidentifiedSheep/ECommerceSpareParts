using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using FluentValidation;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Lrts.RemoveOldAnalyticsRuns;

public record RemoveOldAnalyticsRunsState : RemoveOldAnalyticsRunsInputState;

public record RemoveOldAnalyticsRunsInputState : IInputState
{
	[JsonPropertyName("olderThanDays")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("remove.old.analytics.runs.input.older.than.days.name")]
	[SchemaFieldDescription("remove.old.analytics.runs.input.older.than.days.description")]
	public required int OlderThanDays { get; init; }

	public void ValidateState() => new RemoveOldAnalyticsRunsInputStateValidator().ValidateAndThrow(this);
}
