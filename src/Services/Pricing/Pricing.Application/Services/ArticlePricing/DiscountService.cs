using Pricing.Abstractions.Interfaces.Services.Pricing;

namespace Pricing.Application.Services.ArticlePricing;

public class DiscountService : IDiscountService
{
    public decimal WithDiscount(decimal price, decimal discount) => price - (price * discount / 100);
}