using Pricing.Enums;

namespace Pricing.Application.Models.Pricing;

public record BasePricingResult(IEnumerable<BasePricingItemResult> Items, ProductPricingType PricingType);