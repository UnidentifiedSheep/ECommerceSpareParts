using Pricing.Application.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Interfaces.Services;

public interface IBasePricesService
{
    BasePricingResult CalculatePrices(BasePricingContext context);
    BasePricingItemResult CalculatePrice(BasePricingItem item, ProductPricingType pricingType);
}