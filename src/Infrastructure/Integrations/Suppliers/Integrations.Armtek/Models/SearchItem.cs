using System.Globalization;
using System.Text.Json.Serialization;
using Integrations.Armtek.Converters;

namespace Integrations.Armtek.Models;

public record SearchItem
{
	private const string DateFormat = "yyyyMMddHHmmss";

	private const string OwnWarehousePartnerCode = "0";

	private const string NotAnalogFlag = "0";

	private static readonly TimeSpan ArmtekOffset = TimeSpan.FromHours(3);

	[JsonPropertyName("PIN")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Number { get; init; }

	[JsonPropertyName("BRAND")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Brand { get; init; }

	[JsonPropertyName("NAME")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Name { get; init; }

	[JsonPropertyName("ARTID")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? ArticleId { get; init; }

	[JsonPropertyName("PARNR")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? PartnerCode { get; init; }

	[JsonPropertyName("KEYZAK")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? WarehouseCode { get; init; }

	[JsonPropertyName("RVALUE")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? AvailableQuantityRaw { get; init; }

	[JsonPropertyName("RETDAYS")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? DaysToRefundRaw { get; init; }

	[JsonPropertyName("RDPRF")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? PackQuantityRaw { get; init; }

	[JsonPropertyName("MINBM")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? MinimumOrderQuantityRaw { get; init; }

	[JsonPropertyName("VENSL")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? DeliveryProbabilityRaw { get; init; }

	[JsonPropertyName("PRICE")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? PriceRaw { get; init; }

	[JsonPropertyName("WAERS")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? Currency { get; init; }

	[JsonPropertyName("DLVDT")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? DeliveryDateRaw { get; init; }

	[JsonPropertyName("WRNTDT")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? GuaranteedDeliveryDateRaw { get; init; }

	[JsonPropertyName("ANALOG")]
	[JsonConverter(typeof(FlexibleStringConverter))]
	public string? AnalogFlag { get; init; }

	[JsonIgnore]
	public int AvailableQuantity => ParseInt(AvailableQuantityRaw?.TrimStart('>', '<', '=', '≥', ' ')) ?? 0;

	[JsonIgnore]
	public int DaysToRefund => ParseInt(DaysToRefundRaw) ?? 0;

	[JsonIgnore]
	public int PackQuantity => Math.Max(ParseInt(PackQuantityRaw) ?? 1, 1);

	[JsonIgnore]
	public int MinimumOrderQuantity => Math.Max(ParseInt(MinimumOrderQuantityRaw) ?? 1, 1);

	[JsonIgnore]
	public int? DeliveryProbability
		=> ParseDecimal(DeliveryProbabilityRaw) is { } value ? (int)Math.Clamp(Math.Round(value), 0, 100) : null;

	[JsonIgnore]
	public decimal? Price => ParseDecimal(PriceRaw);

	[JsonIgnore]
	public DateTimeOffset? DeliveryDate => ParseDate(DeliveryDateRaw);

	[JsonIgnore]
	public DateTimeOffset? GuaranteedDeliveryDate => ParseDate(GuaranteedDeliveryDateRaw);

	[JsonIgnore]
	public bool IsPartnerWarehouse
		=> !string.IsNullOrWhiteSpace(PartnerCode) && PartnerCode.Trim() != OwnWarehousePartnerCode;

	[JsonIgnore]
	public bool IsAnalog => !string.IsNullOrWhiteSpace(AnalogFlag) && AnalogFlag.Trim() != NotAnalogFlag;

	private static int? ParseInt(string? value)
		=> ParseDecimal(value) is { } result ? (int)Math.Floor(result) : null;

	private static decimal? ParseDecimal(string? value)
		=> decimal.TryParse(
			value?.Replace(" ", string.Empty).Replace(',', '.'),
			NumberStyles.Number,
			CultureInfo.InvariantCulture,
			out var result)
			? result
			: null;

	private static DateTimeOffset? ParseDate(string? value)
		=> DateTime.TryParseExact(
			value?.Trim(),
			DateFormat,
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var local)
			? new DateTimeOffset(local, ArmtekOffset)
			: null;
}
