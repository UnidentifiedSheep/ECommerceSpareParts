using System.Text.Json.Serialization;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Abstractions.Models;

public record AbcXyzParameters
{
	public const decimal DefaultAdiCutoff = 1.32m;

	public const decimal DefaultCv2Cutoff = 0.49m;

	[JsonPropertyName("from")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	[SchemaFieldLabel("abc.xyz.parameters.from.name")]
	[SchemaFieldDescription("abc.xyz.parameters.from.description")]
	public required DateTime From { get; init; }

	[JsonPropertyName("to")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.MonthPicker)]
	[SchemaFieldLabel("abc.xyz.parameters.to.name")]
	[SchemaFieldDescription("abc.xyz.parameters.to.description")]
	public required DateTime To { get; init; }

	[JsonPropertyName("months")]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.Month)]
	[SchemaFieldLabel("abc.xyz.parameters.months.name")]
	[SchemaFieldDescription("abc.xyz.parameters.months.description")]
	public IReadOnlyCollection<Month>? Months { get; init; }

	[JsonPropertyName("abcMetric")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.AnalysisMetric)]
	[SchemaFieldLabel("abc.xyz.parameters.abc.metric.name")]
	[SchemaFieldDescription("abc.xyz.parameters.abc.metric.description")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required AnalysisMetric AbcMetric { get; init; }

	[JsonPropertyName("xyzMetric")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.AnalysisMetric)]
	[SchemaFieldLabel("abc.xyz.parameters.xyz.metric.name")]
	[SchemaFieldDescription("abc.xyz.parameters.xyz.metric.description")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required AnalysisMetric XyzMetric { get; init; }

	[JsonPropertyName("aShare")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.a.share.name")]
	[SchemaFieldDescription("abc.xyz.parameters.a.share.description")]
	public required decimal AShare { get; init; }

	[JsonPropertyName("bShare")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.b.share.name")]
	[SchemaFieldDescription("abc.xyz.parameters.b.share.description")]
	public required decimal BShare { get; init; }

	[JsonPropertyName("xCoefficient")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.x.coefficient.name")]
	[SchemaFieldDescription("abc.xyz.parameters.x.coefficient.description")]
	public required decimal XCoefficient { get; init; }

	[JsonPropertyName("yCoefficient")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.y.coefficient.name")]
	[SchemaFieldDescription("abc.xyz.parameters.y.coefficient.description")]
	public required decimal YCoefficient { get; init; }

	[JsonPropertyName("minPeriods")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.min.periods.name")]
	[SchemaFieldDescription("abc.xyz.parameters.min.periods.description")]
	public required int MinPeriods { get; init; }

	[JsonPropertyName("granularity")]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(SchemaEntity.AnalysisPeriodGranularity)]
	[SchemaFieldLabel("abc.xyz.parameters.granularity.name")]
	[SchemaFieldDescription("abc.xyz.parameters.granularity.description")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public AnalysisPeriodGranularity Granularity { get; init; } = AnalysisPeriodGranularity.Month;

	[JsonPropertyName("adiCutoff")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.adi.cutoff.name")]
	[SchemaFieldDescription("abc.xyz.parameters.adi.cutoff.description")]
	public decimal AdiCutoff { get; init; } = DefaultAdiCutoff;

	[JsonPropertyName("cv2Cutoff")]
	[SchemaInputControl(InputControlType.TextField)]
	[SchemaFieldLabel("abc.xyz.parameters.cv2.cutoff.name")]
	[SchemaFieldDescription("abc.xyz.parameters.cv2.cutoff.description")]
	public decimal Cv2Cutoff { get; init; } = DefaultCv2Cutoff;

	[JsonPropertyName("organizationId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.Organization, "id")]
	[SchemaFieldLabel("abc.xyz.parameters.organization.id.name")]
	[SchemaFieldDescription("abc.xyz.parameters.organization.id.description")]
	public Guid? OrganizationId { get; init; }

	[JsonPropertyName("buyerId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity(SchemaEntity.User, "id")]
	[SchemaFieldLabel("abc.xyz.parameters.buyer.id.name")]
	[SchemaFieldDescription("abc.xyz.parameters.buyer.id.description")]
	public Guid? BuyerId { get; init; }

	public DateTime EndExclusive() => To.AddMonths(1);

	public IReadOnlyList<DateTime> Periods()
	{
		var quarterly = Granularity == AnalysisPeriodGranularity.Quarter;
		var periods = new List<DateTime>();
		for (var period = From; period < EndExclusive(); period = period.AddMonths(quarterly ? 3 : 1))
			if (quarterly || Months is not { Count: > 0 } || Months.Contains((Month)period.Month))
				periods.Add(period);

		return periods;
	}

	public DateTime PeriodOf(DateTime monthStart)
		=> Granularity == AnalysisPeriodGranularity.Quarter
			? new DateTime(
				monthStart.Year,
				(monthStart.Month - 1) / 3 * 3 + 1,
				1,
				0,
				0,
				0,
				DateTimeKind.Utc)
			: monthStart;
}
