using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Abstractions.Interfaces.Services;

public interface IBasePricesService
{
    BasePricingResult CalculatePrices(BasePricingContext context);
    BasePricingItemResult CalculatePrice(BasePricingItem item, ArticlePricingType pricingType);
}