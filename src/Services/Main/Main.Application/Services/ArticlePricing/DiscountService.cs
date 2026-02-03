using Main.Abstractions.Interfaces.Pricing;

namespace Main.Application.Services.ArticlePricing;

public class DiscountService : IDiscountService
{
    public decimal WithDiscount(decimal price, decimal discount) => price - (price * discount / 100);
}