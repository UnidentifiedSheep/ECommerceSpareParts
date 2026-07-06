using System.Text.Json.Serialization;
using Enums;
using Enums.Units;
using Main.Enums;

namespace Main.Application.Dtos.Logistics;

public record DeliveryCostDto
{
    [JsonPropertyName("items")]
    public required List<DeliveryCostItemDto> Items { get; init; }

    [JsonPropertyName("totalAreaM3")]
    public required decimal TotalAreaM3 { get; init; }

    [JsonPropertyName("totalWeight")]
    public required decimal TotalWeight { get; init; }

    [JsonPropertyName("weightUnit")]
    public required WeightUnit WeightUnit { get; init; }

    [JsonPropertyName("totalCost")]
    public required decimal TotalCost { get; init; }

    [JsonPropertyName("minimalPrice")]
    public required decimal MinimalPrice { get; init; }

    [JsonPropertyName("minimalPriceApplied")]
    public required bool MinimalPriceApplied { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("pricingModel")]
    public required LogisticPricingType PricingModel { get; init; }
}