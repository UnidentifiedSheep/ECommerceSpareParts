using Main.Abstractions.Models.Pricing;

namespace Main.Abstractions.Interfaces.Pricing;

public interface IPriceService
{
    PricingResult GetPrice(PricingContext context);
}