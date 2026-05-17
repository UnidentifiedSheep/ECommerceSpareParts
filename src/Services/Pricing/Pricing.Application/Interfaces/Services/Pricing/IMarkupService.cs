using Pricing.Entities;

namespace Pricing.Application.Interfaces.Services.Pricing;

public interface IMarkupService
{
    Task SetUp(MarkupGroup markupGroup);
    decimal GetMarkup(decimal value, int currencyId);
    decimal WithMarkup(decimal value, decimal markupFraction);
}