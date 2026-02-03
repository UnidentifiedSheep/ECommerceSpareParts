namespace Main.Abstractions.Interfaces.Pricing;

public interface IDiscountService
{
    decimal WithDiscount(decimal price, decimal discount);
}