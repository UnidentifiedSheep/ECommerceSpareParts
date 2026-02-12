using Abstractions.Interfaces;
using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Pricing.Abstractions.Constants;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models;
using Pricing.Entities;

namespace Pricing.Application.Services.ArticlePricing;

public class MarkupService(ICurrencyConverter currencyConverter, ISettingsContainer settingsContainer) : IMarkupService
{
    private readonly Dictionary<int, AdaptiveIntervalMap<MarkupModel>> _markUps = new();
    private decimal _defaultMarkUp;

    public void SetUp(MarkupGroup group)
    {
        _markUps.Clear();
        var setting = settingsContainer.GetSetting(Settings.Pricing);
        _defaultMarkUp = setting.DefaultMarkup;

        foreach (var toUsd in currencyConverter.ToUsdDoub)
        {
            var intervalMap = new AdaptiveIntervalMap<MarkupModel>(2, true);
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
            var anyMap = _markUps.FirstOrDefault();
            if (anyMap.Value == null) return _defaultMarkUp;
            val = currencyConverter.ConvertTo(val, currencyId, anyMap.Key);
            map = anyMap.Value;
        }

        var markup = map.GetInterval(val);
        return markup?.Value?.Markup ?? _defaultMarkUp;
    }

    public decimal WithMarkup(decimal value, decimal markupFraction)
    {
        return value * (1 + markupFraction);
    }
}