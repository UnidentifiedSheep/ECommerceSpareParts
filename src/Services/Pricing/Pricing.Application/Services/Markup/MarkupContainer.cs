using Application.Common.Services;
using Pricing.Application.Interfaces.Markup;
using Pricing.Entities;
using System.Text.Json;

namespace Pricing.Application.Services.Markup;

public class MarkupContainer : IMarkupContainer
{
    private readonly Lock _lock = new();
    private MarkupRangeIndex _defaultMarkupMap = null!;
    private Dictionary<int, MarkupRangeIndex> _markupMaps = new();
    public bool Initialized { get; private set; }
    public Models.Markup DefaultMarkup { get; private set; } = null!;
    public int DefaultCurrencyId { get; private set; }
    public string CurrentVersion { get; private set; } = string.Empty;

    public Models.Markup? GetForDefaultOrNull(decimal value)
    {
        EnsureInitialized();
        return _defaultMarkupMap.Get(value);
    }

    public Models.Markup? GetForCurrencyOrNull(int currencyId, decimal value)
    {
        EnsureInitialized();
        return _markupMaps.TryGetValue(currencyId, out var map)
            ? map.Get(value)
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
            var defaultRanges = @default.ToList();
            var otherRanges = other.ToDictionary(
                x => x.Key,
                x => x.Value.ToList());
            
            Initialized = true;
            DefaultMarkup = defaultMarkup;
            DefaultCurrencyId = defaultCurrencyId;
            _defaultMarkupMap = GenMap(defaultRanges);
            _markupMaps = otherRanges.ToDictionary(x => x.Key, x => GenMap(x.Value));
            GenMarkupHash(defaultRanges, otherRanges, defaultCurrencyId, defaultMarkup);
        }
    }

    private static MarkupRangeIndex GenMap(IEnumerable<MarkupRange> markupRanges)
    {
        return new MarkupRangeIndex(
            markupRanges
                .OrderBy(x => x.RangeStart)
                .ThenBy(x => x.RangeEnd)
                .Select(x => new IndexedMarkupRange(
                    x.RangeStart,
                    x.RangeEnd,
                    new Models.Markup(x.Markup)))
                .ToArray());
    }

    private void GenMarkupHash(
        IReadOnlyCollection<MarkupRange> defaultRanges,
        IReadOnlyDictionary<int, List<MarkupRange>> otherRanges,
        int defaultCurrencyId,
        Models.Markup defaultMarkup)
    {
        CurrentVersion = ConfigurationVersionGenerator.Generate(writer =>
        {
            writer.WriteStartObject();
            writer.WriteNumber("defaultCurrencyId", defaultCurrencyId);
            writer.WriteNumber("defaultMarkup", defaultMarkup.Value);
            writer.WritePropertyName("defaultRanges");
            WriteRanges(writer, defaultRanges);
            writer.WriteStartArray("otherCurrencies");

            foreach (var (currencyId, ranges) in otherRanges.OrderBy(x => x.Key))
            {
                writer.WriteStartObject();
                writer.WriteNumber("currencyId", currencyId);
                writer.WritePropertyName("ranges");
                WriteRanges(writer, ranges);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        });
    }

    private static void WriteRanges(
        Utf8JsonWriter writer,
        IEnumerable<MarkupRange> ranges)
    {
        writer.WriteStartArray();

        foreach (var range in ranges
                     .OrderBy(x => x.RangeStart)
                     .ThenBy(x => x.RangeEnd)
                     .ThenBy(x => x.Markup))
        {
            writer.WriteStartObject();
            writer.WriteNumber("rangeStart", range.RangeStart);
            writer.WriteNumber("rangeEnd", range.RangeEnd);
            writer.WriteNumber("markup", range.Markup);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private void EnsureInitialized()
    {
        if (!Initialized) throw new InvalidOperationException("Markup map container is not initialized");
    }

    private sealed class MarkupRangeIndex(IndexedMarkupRange[] ranges)
    {
        public Models.Markup? Get(decimal value)
        {
            var left = 0;
            var right = ranges.Length - 1;
            var candidate = -1;

            while (left <= right)
            {
                var middle = left + ((right - left) >> 1);

                if (ranges[middle].Start <= value)
                {
                    candidate = middle;
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }

            return candidate >= 0 && value <= ranges[candidate].End
                ? ranges[candidate].Markup
                : null;
        }
    }

    private readonly record struct IndexedMarkupRange(
        decimal Start,
        decimal End,
        Models.Markup Markup);
}
