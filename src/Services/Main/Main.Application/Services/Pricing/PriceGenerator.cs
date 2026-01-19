using Core.Interfaces;
using Core.Models;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models;
using Main.Application.Extensions;
using Main.Entities;

namespace Main.Application.Services.Pricing;

public class PriceGenerator(ICurrencyConverter currencyConverter) : IPriceGenerator
{
    private readonly Dictionary<int, AdaptiveIntervalMap<MarkupModel>> _markUps = new();
    public int DefaultMarkUpCurrencyId { get; private set; }
    private double DefaultMarkUp => 20;
    private double MinimalMarkUp => 6;

    public double GetSellPriceWithMinimalMarkUp(double buyPrice)
    {
        var sellPrice = GetSellPrice(buyPrice, 0, MinimalMarkUp);
        return sellPrice;
    }

    public void SetUp(MarkupGroup group, Settings settings)
    {
        _markUps.Clear();
        DefaultMarkUpCurrencyId = settings.DefaultCurrency;

        foreach (var toUsd in currencyConverter.ToUsdDoub)
        {
            var intervalMap = new AdaptiveIntervalMap<MarkupModel>();
            var lastRangeEnd = double.NaN;

            foreach (var range in group.MarkupRanges.OrderBy(x => x.RangeStart))
            {
                var rangeStart = double.IsNaN(lastRangeEnd)
                    ? Math.Round(Convert.ToDouble(range.RangeStart), 2)
                    : Math.Round(lastRangeEnd + 0.02, 2);

                var exchangedEnd = (double)range.RangeEnd / currencyConverter.ToUsdDoub[group.CurrencyId] * toUsd.Value;
                var rangeEnd = Math.Round(exchangedEnd, 2);
                var markupModel = new MarkupModel(Math.Round(Convert.ToDouble(range.Markup), 2));

                intervalMap.AddInterval(new Interval<MarkupModel>(rangeStart, rangeEnd, markupModel));
                lastRangeEnd = rangeEnd;
            }

            _markUps[toUsd.Key] = intervalMap;
        }
    }

    /// <summary>
    ///     return sell price for given buy price.
    /// </summary>
    /// <param name="buyPrice">buy price</param>
    /// <param name="discount">discount in percentage</param>
    /// <param name="currencyId">the id of currency</param>
    /// <returns></returns>
    public double GetSellPrice(double buyPrice, double discount, int currencyId)
    {
        var markup = GetMarkup(buyPrice, currencyId);
        var sellPrice = GetSellPrice(buyPrice, discount, markup);
        return sellPrice;
    }

    /// <summary>
    ///     Returns list of buy and sell prices, for list of buy prices.
    /// </summary>
    /// <param name="buyPrices">List of buy prices</param>
    /// <param name="discount">Discount in percentage</param>
    /// <param name="currencyId">The currency of buy price</param>
    /// <returns> Where key buyPrice and value is Sell Price. </returns>
    public Dictionary<double, double> GetSellPrice(IEnumerable<double> buyPrices, double discount, int currencyId)
    {
        var res = new Dictionary<double, double>();
        var foundMap = _markUps.TryGetValue(currencyId, out var map);
        if (!foundMap)
            map = _markUps[DefaultMarkUpCurrencyId];

        foreach (var price in buyPrices)
        {
            var value = price;
            if (res.ContainsKey(value)) continue;
            if (!foundMap)
                value = currencyConverter.ConvertTo(value, currencyId, DefaultMarkUpCurrencyId);
            var markup = GetMarkup(value, map!);
            var sellPrice = GetSellPrice(value, discount, markup);
            res.Add(value, sellPrice);
        }

        return res;
    }

    public double GetDiscountFromPrices(double withDiscount, double withNoDiscount)
    {
        return (withNoDiscount - withDiscount) / withNoDiscount * 100;
    }

    public decimal GetDiscountFromPrices(decimal withDiscount, decimal withNoDiscount)
    {
        return (withNoDiscount - withDiscount) / withNoDiscount * 100;
    }

    private double GetSellPrice(double buyPrice, double discount, double markup)
    {
        return buyPrice.GetMarkUppedPrice(markup).GetDiscountedPrice(discount).RoundToNearestUp();
    }

    /// <summary>
    ///     Get markup from interval map via buy price.
    /// </summary>
    /// <param name="value">Is buy price</param>
    /// <param name="currencyId">Id of buy price</param>
    /// <returns>Markup value for this price</returns>
    private double GetMarkup(double value, int currencyId)
    {
        if (_markUps.Count == 0) return DefaultMarkUp;
        var foundMap = _markUps.TryGetValue(currencyId, out var map);
        if (!foundMap)
        {
            map = _markUps[DefaultMarkUpCurrencyId];
            value = currencyConverter.ConvertTo(value, currencyId, DefaultMarkUpCurrencyId);
        }

        var markup = map!.GetInterval(value);
        if (markup == null || markup.Value == null) return DefaultMarkUp;
        return markup.Value!.Markup;
    }

    /// <summary>
    ///     Get markup from interval map via buy price.
    /// </summary>
    /// <param name="value">Is buy price</param>
    /// <param name="map">Map in which markup should be found</param>
    /// <returns>Markup for this price</returns>
    private double GetMarkup(double value, AdaptiveIntervalMap<MarkupModel> map)
    {
        var markup = map.GetInterval(value);
        if (markup == null || markup.Value == null) return DefaultMarkUp;
        return markup.Value!.Markup;
    }
}