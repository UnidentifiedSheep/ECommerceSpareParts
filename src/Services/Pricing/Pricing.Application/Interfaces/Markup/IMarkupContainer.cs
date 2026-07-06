using Pricing.Entities;

namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupContainer
{
    bool Initialized { get; }

    Models.Markup DefaultMarkup { get; }
    int DefaultCurrencyId { get; }
    Models.Markup? GetForDefaultOrNull(double value);
    Models.Markup? GetForCurrencyOrNull(int currencyId, double value);

    void Initialize(
        int defaultCurrencyId,
        Models.Markup defaultMarkup,
        IEnumerable<MarkupRange> @default,
        Dictionary<int, IEnumerable<MarkupRange>> other);
}