namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupCalculator
{
    decimal GetMarkup(decimal basePrice, int currencyId);
}