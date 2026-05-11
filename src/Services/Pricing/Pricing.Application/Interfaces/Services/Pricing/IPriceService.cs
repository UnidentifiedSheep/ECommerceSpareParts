using Pricing.Abstractions.Models.Pricing;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IPriceService
{
    PricingResult GetPrice(PricingContext context);
}