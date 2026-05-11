using Pricing.Enums;

namespace Pricing.Abstractions.Models.Pricing;

public record BasePricingResult(IEnumerable<BasePricingItemResult> Items, ProductPricingType PricingType);