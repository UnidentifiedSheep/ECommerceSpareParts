using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Abstractions.Models.CoPurchase;

public record CoPurchaseParameters
{
	public const int DefaultMinPairCount = 3;

	public const int DefaultSuspiciousCrossMinPairCount = 3;

	public const decimal DefaultSuspiciousCrossMinLift = 2m;

	public const int DefaultCandidatesTopK = 50;

	public const int DefaultResultTopN = 10;

	[JsonPropertyName("from")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	[SchemaFieldLabel("co.purchase.parameters.from.name")]
	[SchemaFieldDescription("co.purchase.parameters.from.description")]
	public required DateTime From { get; init; }

	[JsonPropertyName("to")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	[SchemaFieldLabel("co.purchase.parameters.to.name")]
	[SchemaFieldDescription("co.purchase.parameters.to.description")]
	public required DateTime To { get; init; }

	[JsonPropertyName("minPairCount")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("co.purchase.parameters.min.pair.count.name")]
	[SchemaFieldDescription("co.purchase.parameters.min.pair.count.description")]
	public int MinPairCount { get; init; } = DefaultMinPairCount;

	[JsonPropertyName("suspiciousCrossMinPairCount")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("co.purchase.parameters.suspicious.cross.min.pair.count.name")]
	[SchemaFieldDescription("co.purchase.parameters.suspicious.cross.min.pair.count.description")]
	public int SuspiciousCrossMinPairCount { get; init; } = DefaultSuspiciousCrossMinPairCount;

	[JsonPropertyName("suspiciousCrossMinLift")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("co.purchase.parameters.suspicious.cross.min.lift.name")]
	[SchemaFieldDescription("co.purchase.parameters.suspicious.cross.min.lift.description")]
	public decimal SuspiciousCrossMinLift { get; init; } = DefaultSuspiciousCrossMinLift;

	[JsonPropertyName("candidatesTopK")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("co.purchase.parameters.candidates.top.k.name")]
	[SchemaFieldDescription("co.purchase.parameters.candidates.top.k.description")]
	public int CandidatesTopK { get; init; } = DefaultCandidatesTopK;

	[JsonPropertyName("resultTopN")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("co.purchase.parameters.result.top.n.name")]
	[SchemaFieldDescription("co.purchase.parameters.result.top.n.description")]
	public int ResultTopN { get; init; } = DefaultResultTopN;

	public DateTime EndExclusive() => To.AddMonths(1);
}
