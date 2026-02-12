using Main.Abstractions.Models.Pricing;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IPriceService
{
    PricingResult GetPrice(PricingContext context);
}