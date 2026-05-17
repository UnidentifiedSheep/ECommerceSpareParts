using Pricing.Application.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Services.Pricing;

public interface IBasePriceStrategy
{
    ProductPricingType Type { get; }
    decimal GetPrice(IEnumerable<ProductPrice> prices);
}