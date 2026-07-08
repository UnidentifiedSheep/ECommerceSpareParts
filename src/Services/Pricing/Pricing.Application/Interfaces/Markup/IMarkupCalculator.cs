using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupCalculator
{
    MarkupResult GetMarkup(decimal basePrice, int currencyId);
}