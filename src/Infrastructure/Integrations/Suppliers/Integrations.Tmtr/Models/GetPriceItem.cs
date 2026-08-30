using System.Globalization;
using System.Text.Json.Serialization;
using Integrations.Tmtr.Enums;

namespace Integrations.Tmtr.Models;

public sealed record GetPriceItem
{
	[JsonPropertyName("CrossID")]
	public int ProductId { get; init; }

	[JsonPropertyName("DeadLine")]
	public string? AvailabilityDeadlineRaw { get; init; }

	[JsonIgnore]
	public DateTimeOffset? AvailabilityDeadline => ParseDate(AvailabilityDeadlineRaw);

	[JsonIgnore]
	public int? AvailabilityPeriodDays =>
		int.TryParse(
			AvailabilityDeadlineRaw,
			NumberStyles.Integer,
			CultureInfo.InvariantCulture,
			out var days)
			? days
			: null;

	[JsonPropertyName("OS")]
	public OfferLocationType LocationType { get; init; }

	[JsonPropertyName("ShowedQuantity")]
	public string? AvailableQuantityRaw { get; init; }

	[JsonIgnore]
	public int AvailableQuantity => ParseDisplayedQuantity(AvailableQuantityRaw);

	[JsonPropertyName("DeliveryDate")]
	public string? ExpectedDeliveryDateRaw { get; init; }

	[JsonIgnore]
	public DateTimeOffset? ExpectedDeliveryDate => ParseDate(ExpectedDeliveryDateRaw);

	[JsonPropertyName("MinPartyQuantity")]
	public int MinimumOrderQuantity { get; init; }

	[JsonPropertyName("MinPackQuantity")]
	public int MinimumPackQuantity { get; init; }

	[JsonPropertyName("GuarantedDate")]
	public string? GuaranteedDeliveryDateRaw { get; init; }

	[JsonIgnore]
	public DateTimeOffset? GuaranteedDeliveryDate => ParseDate(GuaranteedDeliveryDateRaw);

	[JsonPropertyName("PriceLastUpdateDate")]
	public string? PriceUpdatedAtRaw { get; init; }

	[JsonIgnore]
	public DateTimeOffset? PriceUpdatedAt => ParseDate(PriceUpdatedAtRaw);

	[JsonPropertyName("Ver")]
	public double RecordVersion { get; init; }

	[JsonPropertyName("VerVsrok")]
	public double DeliveryTermsVersion { get; init; }

	[JsonPropertyName("HashCode")]
	public int SearchResultHash { get; init; }

	[JsonPropertyName("Article")]
	public string Number { get; init; } = null!;

	[JsonPropertyName("Producer")]
	public string Brand { get; init; } = null!;

	[JsonPropertyName("Nomenclature")]
	public string ProductName { get; init; } = null!;

	[JsonPropertyName("Price")]
	public decimal UnitPrice { get; init; }

	[JsonPropertyName("StockIdentifier")]
	public string WarehouseIdentifier { get; init; } = null!;

	[JsonPropertyName("StockName")]
	public string WarehouseName { get; init; } = null!;

	[JsonPropertyName("PriceId")]
	public int PriceListId { get; init; }

	[JsonPropertyName("PriceDetailId")]
	public long PriceListItemId { get; init; }

	[JsonPropertyName("StorageCode")]
	public string? StorageLocationCode { get; init; }

	[JsonPropertyName("DeliveryPeriod")]
	public int DeliveryDays { get; init; }

	[JsonPropertyName("OrderIdentify")]
	public string? OrderIdentifier { get; init; }

	private static DateTimeOffset? ParseDate(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		return DateTimeOffset.TryParse(
			value,
			CultureInfo.InvariantCulture,
			DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
			out var result)
			? result
			: null;
	}

	private static int ParseDisplayedQuantity(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return 0;

		var numericPart = value.TrimStart('>', ' ');

		return int.TryParse(
			numericPart,
			NumberStyles.Integer,
			CultureInfo.InvariantCulture,
			out var quantity)
			? quantity
			: 0;
	}
}
