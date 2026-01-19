using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Abstractions.Interfaces.Pricing;

public interface IPriceGenerator
{
    Dictionary<double, double> GetSellPrice(IEnumerable<double> buyPrices, double discount, int currencyId);
    double GetSellPrice(double buyPrice, double discount, int currencyId);
    double GetSellPriceWithMinimalMarkUp(double buyPrice);

    void SetUp(MarkupGroup markupGroup, Settings settings);

    double GetDiscountFromPrices(double withDiscount, double withNoDiscount);
    decimal GetDiscountFromPrices(decimal withDiscount, decimal withNoDiscount);
}