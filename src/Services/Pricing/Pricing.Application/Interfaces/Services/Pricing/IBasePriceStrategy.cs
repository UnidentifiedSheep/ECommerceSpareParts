using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IBasePriceStrategy
{
    ProductPricingType Type { get; }
    decimal GetPrice(IEnumerable<ProductPrice> prices);
}