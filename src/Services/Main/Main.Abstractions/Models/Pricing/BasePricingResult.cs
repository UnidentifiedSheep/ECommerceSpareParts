using Main.Enums;

namespace Main.Abstractions.Models.Pricing;

public record BasePricingResult(IEnumerable<BasePricingItemResult> Items, ArticlePricingType PricingType);