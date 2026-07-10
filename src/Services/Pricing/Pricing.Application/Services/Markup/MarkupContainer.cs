using IntervalMap.Core.Abstractions;
using IntervalMap.Core.Models;
using IntervalMap.Variations;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Markup;
using Pricing.Entities;

namespace Pricing.Application.Services.Markup;

public class MarkupContainer : IMarkupContainer
{
    private readonly Lock _lock = new();
    private IntervalMapBase<Interval<Models.Markup>> _defaultMarkupMap = null!;
    private Dictionary<int, IntervalMapBase<Interval<Models.Markup>>> _markupMaps = new();
    public bool Initialized { get; private set; }
    public Models.Markup DefaultMarkup { get; private set; } = null!;
    public int DefaultCurrencyId { get; private set; }

    public Models.Markup? GetForDefaultOrNull(double value)
    {
        EnsureInitialized();
        return _defaultMarkupMap.GetInterval(value)?.Value;
    }

    public Models.Markup? GetForCurrencyOrNull(int currencyId, double value)
    {
        EnsureInitialized();
        return _markupMaps.TryGetValue(currencyId, out var map)
            ? map.GetInterval(value)?.Value
            : null;
    }

    public void Initialize(
        int defaultCurrencyId,
        Models.Markup defaultMarkup,
        IEnumerable<MarkupRange> @default,
        Dictionary<int, IEnumerable<MarkupRange>> other)
    {
        lock (_lock)
        {
            Initialized = true;
            DefaultMarkup = defaultMarkup;
            DefaultCurrencyId = defaultCurrencyId;
            _defaultMarkupMap = GenMap(@default);
            _markupMaps = other.ToDictionary(x => x.Key, x => GenMap(x.Value));
        }
    }

    private static IntervalMapBase<Interval<Models.Markup>> GenMap(IEnumerable<MarkupRange> markupRanges)
    {
        var map = new AdaptiveIntervalMap<Models.Markup>(intersectionAllowed: true);

        foreach (var range in markupRanges)
            map.AddInterval(
                new Interval<Models.Markup>(
                    (double)range.RangeStart,
                    (double)range.RangeEnd,
                    new Models.Markup(range.Markup)));

        return map;
    }

    private void EnsureInitialized()
    {
        if (!Initialized) throw new InvalidOperationException("Markup map container is not initialized");
    }
}