using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Abstractions.Interfaces.Pricing;

public interface IMarkupService
{
    void SetUp(MarkupGroup markupGroup, Settings settings);
    decimal GetMarkup(decimal value, int currencyId);
}