using Main.Enums;

namespace Main.Abstractions.Models.Pricing;

public class BasePricingContext(IEnumerable<BasePricingItem> items, ArticlePricingType pricingType)
{
    public IReadOnlyList<BasePricingItem> Items { get; } = items.ToList();
    public ArticlePricingType PricingType => pricingType;
}