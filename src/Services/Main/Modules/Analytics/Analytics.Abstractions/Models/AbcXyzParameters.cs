using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Abstractions.Models;

public sealed record AbcXyzParameters
{
	[JsonPropertyName("from")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	public required DateTime From { get; init; }

	[JsonPropertyName("to")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	public required DateTime To { get; init; }

	public IReadOnlyCollection<Month>? Months { get; init; }

	public required AnalysisMetric AbcMetric { get; init; }

	public required AnalysisMetric XyzMetric { get; init; }

	public required decimal AShare { get; init; }

	public required decimal BShare { get; init; }

	public required decimal XCoefficient { get; init; }

	public required decimal YCoefficient { get; init; }

	public required int MinPeriods { get; init; }

	public Guid? OrganizationId { get; init; }

	public Guid? BuyerId { get; init; }

	public DateTime EndExclusive() => To.AddMonths(1);
}
