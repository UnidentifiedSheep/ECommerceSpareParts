using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Services.Pricing;

public interface IPriceService
{
    PricingResult GetPrice(PricingContext context);
}