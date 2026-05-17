using Pricing.Application.Interfaces.Services.Pricing;

namespace Pricing.Application.Services.ProductPricing;

public class DiscountService : IDiscountService
{
    public decimal WithDiscount(decimal price, decimal discount)
    {
        return price - price * discount / 100;
    }
}