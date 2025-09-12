using Core.Entities;
using Core.Models;

namespace Core.Interfaces;

public interface IPriceGenerator
{
    Dictionary<double, double> GetSellPrice(IEnumerable<double> buyPrices, double discount, int currencyId);
    double GetSellPrice(double buyPrice, double discount, int currencyId);
    double GetSellPriceWithMinimalMarkUp(double buyPrice);

    void SetUp(MarkupGroup markupGroup, DefaultSettings defaultSettings);

    double GetDiscountFromPrices(double withDiscount, double withNoDiscount);
    decimal GetDiscountFromPrices(decimal withDiscount, decimal withNoDiscount);
}