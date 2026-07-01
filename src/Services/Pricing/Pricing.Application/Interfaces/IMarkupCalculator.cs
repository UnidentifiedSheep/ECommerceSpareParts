namespace Pricing.Application.Interfaces;

public interface IMarkupCalculator
{
    decimal GetMarkup(decimal basePrice, int currencyId);
}