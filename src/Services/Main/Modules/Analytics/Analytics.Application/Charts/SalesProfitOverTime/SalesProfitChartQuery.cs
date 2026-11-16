using System.Text.Json.Serialization;
using Domain.Validation;
using Main.Entities;
using Main.Entities.Analytics.Enums;
using Main.Modules.Analytics.Abstractions.Charts;
using SchemaGeneration.Abstractions.Attributes;
using SchemaGeneration.Abstractions.Enums;

namespace Main.Modules.Analytics.Application.Charts.SalesProfitOverTime;

public sealed record SalesProfitChartQuery : CursorChartQueryInput<DateTime>
{
	[JsonPropertyName("organizationId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity("Organization", "id")]
	[SchemaFieldLabel("chart.sales.profit.query.organization.id.name")]
	[SchemaFieldDescription("chart.sales.profit.query.organization.id.description")]
	public Guid? OrganizationId { get; init; }

	[JsonPropertyName("buyerId")]
	[SchemaInputControl(InputControlType.EntitySelector)]
	[SchemaDependsOnEntity("User", "id")]
	[SchemaFieldLabel("chart.sales.profit.query.buyer.id.name")]
	[SchemaFieldDescription("chart.sales.profit.query.buyer.id.description")]
	public Guid? BuyerId { get; init; }

	[JsonPropertyName("startDate")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.DatePicker)]
	[SchemaFieldLabel("chart.sales.profit.query.start.date.name")]
	[SchemaFieldDescription("chart.sales.profit.query.start.date.description")]
	public required DateTime StartDate { get; init; }

	[JsonPropertyName("endDate")]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.DatePicker)]
	[SchemaFieldLabel("chart.sales.profit.query.end.date.name")]
	[SchemaFieldDescription("chart.sales.profit.query.end.date.description")]
	public required DateTime EndDate { get; init; }

	[JsonPropertyName("granularity")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	[RequiredSchemaField]
	[SchemaInputControl(InputControlType.EnumSelector)]
	[SchemaDependsOnEntity(nameof(TimeGranularity))]
	[SchemaFieldLabel("chart.sales.profit.query.granularity.name")]
	[SchemaFieldDescription("chart.sales.profit.query.granularity.description")]
	public TimeGranularity Granularity { get; init; } = TimeGranularity.Day;

	public void Validate()
	{
		StartDate.Ensure(
			date => date.Kind == DateTimeKind.Utc,
			ChartSalesProfitQueryStartDateMustBeUtcMessage.Instance);
		EndDate.Ensure(
			date => date.Kind == DateTimeKind.Utc,
			ChartSalesProfitQueryEndDateMustBeUtcMessage.Instance);
		StartDate.EnsureAtMost(
			EndDate,
			ChartSalesProfitQueryStartDateMustBeBeforeOrEqualEndDateMessage.Instance);

		Granularity.Ensure(Enum.IsDefined, ChartSalesProfitQueryGranularityUnsupportedMessage.Instance);

		if (Cursor is { } cursor)
			cursor.Ensure(
				value => value.Kind == DateTimeKind.Utc,
				ChartSalesProfitQueryCursorInvalidMessage.Instance);

		ValidateCursor();
	}
}
