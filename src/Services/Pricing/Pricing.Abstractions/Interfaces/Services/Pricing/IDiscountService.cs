namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IDiscountService
{
    decimal WithDiscount(decimal price, decimal discount);
}