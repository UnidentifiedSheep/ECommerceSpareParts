using IntervalMap.IntervalVariations;
using MonoliteUnicorn.Services.Prices.PriceGenerator.Models;

namespace MonoliteUnicorn.Services.Prices.PriceGenerator;

public static class PriceGenerator
{
    public static readonly Dictionary<int, AdaptiveIntervalMap<MarkupModel>> MarkUps = new();
    public static readonly Dictionary<int, decimal> ToUsd = new();
    public static int DefaultMarkUpCurrencyId { get; private set; }
    public static double DefaultMarkUp { get; private set; } = 20;
    public static double MinimalMarkUp { get; private set; } = 6;

    private static double RoundToNearestUp(this double value) => ((int)(value * 100 + 0.5)) / 100.0;

    /// <summary>
    /// Конвертация из выбранной валюты в нужную.
    /// </summary>
    /// <param name="value">Число которое надо конвертировать.</param>
    /// <param name="from">Id валюты из которой нужно конвертировать.</param>
    /// <param name="to">Id валюты в которую нужно конвертировать.</param>
    /// <returns></returns>
    public static double ConvertToNeededCurrency(double value, int from, int to)
    {
        if (from == to) return value;
        var valueInUsd = value / (double)ToUsd[from];
        var converted = valueInUsd * (double)ToUsd[to];
        return converted;
    }
    
    public static double ConvertToUsd(double value, int from)
    {
        if (from == Global.UsdId) return value;
        var valueInUsd = value / (double)ToUsd[from];
        var converted = valueInUsd * (double)ToUsd[Global.UsdId];
        return converted;
    }
    
    public static decimal ConvertToNeededCurrency(decimal value, int from, int to)
    {
        if (from == to) return value;
        var valueInUsd = value / ToUsd[from];
        var converted = valueInUsd * ToUsd[to];
        return converted;
    }
    
    public static decimal ConvertToUsd(decimal value, int from)
    {
        if (from == Global.UsdId) return value;
        var valueInUsd = value / ToUsd[from];
        var converted = valueInUsd * ToUsd[Global.UsdId];
        return converted;
    }
    
    public static double GetSellPriceWithMinimalMarkUp(double buyPrice)
    {
        var sellPrice = GetSellPrice(buyPrice, 0, MinimalMarkUp);
        return sellPrice;
    }
    /// <summary>
    /// return sell price for given buy price.
    /// </summary>
    /// <param name="buyPrice">buy price</param>
    /// <param name="discount">discount in percentage</param>
    /// <param name="currencyId">the id of currency</param>
    /// <returns></returns>
    public static double GetSellPrice(double buyPrice, double discount, int currencyId)
     {
         var markup = GetMarkup(buyPrice, currencyId);
         var sellPrice = GetSellPrice(buyPrice, discount, markup);
         return sellPrice;
    }
    /// <summary>
    /// Returns list of buy and sell prices, for list of buy prices.
    /// </summary>
    /// <param name="buyPrices">List of buy prices</param>
    /// <param name="discount">Discount in percentage</param>
    /// <param name="currencyId">The currency of buy price</param>
    /// <returns> Where key buyPrice and value is Sell Price. </returns>
    public static Dictionary<double, double> GetSellPrice(IEnumerable<double> buyPrices, double discount, int currencyId)
    {
        var res = new Dictionary<double, double>();
        var foundMap = MarkUps.TryGetValue(currencyId, out var map);
        if (!foundMap)
            map = MarkUps[DefaultMarkUpCurrencyId];

        foreach (var price in buyPrices)
        {
            var value = price;
            if (res.ContainsKey(value)) continue;
            if (!foundMap)
                value = ConvertToNeededCurrency(value, currencyId, DefaultMarkUpCurrencyId);
            var markup = GetMarkup(value, map!);
            var sellPrice = GetSellPrice(value, discount, markup);
            res.Add(value, sellPrice);
        }

        return res;
    }

    private static double GetSellPrice(double buyPrice, double discount, double markup) =>
        GetMarkUppedPrice(buyPrice, markup).GetDiscountedPrice(discount).RoundToNearestUp();
    private static double GetMarkUppedPrice(this double price, double markup) => price * (1 + markup / 100);
    private static double GetDiscountedPrice(this double price, double discount) => price * (1 - discount / 100);
    /// <summary>
    /// Get markup from interval map via buy price.
    /// </summary>
    /// <param name="value">Is buy price</param>
    /// <param name="currencyId">Id of buy price</param>
    /// <returns>Markup value for this price</returns>
    private static double GetMarkup(double value, int currencyId)
    {
        if (MarkUps.Count == 0) return DefaultMarkUp;
        var foundMap = MarkUps.TryGetValue(currencyId, out var map);
        if (!foundMap)
        {
            map = MarkUps[DefaultMarkUpCurrencyId];
            value = ConvertToNeededCurrency(value, currencyId, DefaultMarkUpCurrencyId);
        }

        var markup = map!.GetInterval(value);
        if(markup == null || markup.IsNull()) return DefaultMarkUp;
        return markup.Value!.Markup;
    }

    /// <summary>
    /// Get markup from interval map via buy price.
    /// </summary>
    /// <param name="value">Is buy price</param>
    /// <param name="map">Map in which markup should be found</param>
    /// <returns>Markup for this price</returns>
    private static double GetMarkup(double value,  AdaptiveIntervalMap<MarkupModel> map)
    {
        var markup = map.GetInterval(value);
        if(markup == null || markup.IsNull()) return DefaultMarkUp;
        return markup.Value!.Markup;
    }
}

