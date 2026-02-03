using Core.Interfaces;
using Core.Models;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Application.Services.ArticlePricing;

public class MarkupService(ICurrencyConverter currencyConverter) : IMarkupService
{
    private readonly Dictionary<int, AdaptiveIntervalMap<MarkupModel>> _markUps = new();
    public int DefaultMarkUpCurrencyId { get; private set; }
    private decimal _defaultMarkUp;

    public void SetUp(MarkupGroup group, Settings settings)
    {
        _markUps.Clear();
        DefaultMarkUpCurrencyId = settings.DefaultCurrency;
        _defaultMarkUp = settings.DefaultMarkUp;

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
                var markupModel = new MarkupModel(Math.Round(range.Markup, 2));

                intervalMap.AddInterval(new Interval<MarkupModel>(rangeStart, rangeEnd, markupModel));
                lastRangeEnd = rangeEnd;
            }

            _markUps[toUsd.Key] = intervalMap;
        }
    }
    
    public decimal GetMarkup(decimal value, int currencyId)
    {
        double val = Convert.ToDouble(value);
        if (_markUps.Count == 0) return _defaultMarkUp;
        if (!_markUps.TryGetValue(currencyId, out var map))
        {
            if (!_markUps.TryGetValue(DefaultMarkUpCurrencyId, out map))
                throw new ArgumentException("Нет карты наценок для базовой валюты");
            val = currencyConverter.ConvertTo(val, currencyId, DefaultMarkUpCurrencyId);
        }

        var markup = map.GetInterval(val);
        return markup?.Value?.Markup ?? _defaultMarkUp;
    }
}