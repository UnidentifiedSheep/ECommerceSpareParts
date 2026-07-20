using System.Text.Json.Serialization;

namespace Pricing.Application.Models.Pricing;

public sealed record AppliedPriceRule(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("priceBefore")] decimal PriceBefore,
    [property: JsonPropertyName("priceAfter")] decimal PriceAfter);
