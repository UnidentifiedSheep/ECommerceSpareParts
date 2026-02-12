using Pricing.Abstractions.Constants;
using Pricing.Entities;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IMarkupService
{
    void SetUp(MarkupGroup markupGroup);
    decimal GetMarkup(decimal value, int currencyId);
    decimal WithMarkup(decimal value, decimal markupFraction);
}