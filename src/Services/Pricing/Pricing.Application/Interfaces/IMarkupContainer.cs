using Pricing.Application.Models;
using Pricing.Entities;

namespace Pricing.Application.Interfaces;

public interface IMarkupContainer
{
    bool Initialized { get; }

    Markup DefaultMarkup { get; }
    int DefaultCurrencyId { get; }
    Markup? GetForDefaultOrNull(double value);
    Markup? GetForCurrencyOrNull(int currencyId, double value);

    void Initialize(
        int defaultCurrencyId,
        Markup defaultMarkup,
        IEnumerable<MarkupRange> @default,
        Dictionary<int, IEnumerable<MarkupRange>> other);
}