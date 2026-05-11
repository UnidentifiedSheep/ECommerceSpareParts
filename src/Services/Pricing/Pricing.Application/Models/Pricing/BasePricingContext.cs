using Pricing.Enums;

namespace Pricing.Abstractions.Models.Pricing;

public class BasePricingContext(IEnumerable<BasePricingItem> items, ProductPricingType pricingType)
{
    public IReadOnlyList<BasePricingItem> Items { get; } = items.ToList();
    public ProductPricingType PricingType => pricingType;
}