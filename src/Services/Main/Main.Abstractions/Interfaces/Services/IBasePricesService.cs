using Main.Abstractions.Models.Pricing;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Services;

public interface IBasePricesService
{
    BasePricingResult CalculatePrices(BasePricingContext context);
    BasePricingItemResult CalculatePrice(BasePricingItem item, ArticlePricingType pricingType);
}