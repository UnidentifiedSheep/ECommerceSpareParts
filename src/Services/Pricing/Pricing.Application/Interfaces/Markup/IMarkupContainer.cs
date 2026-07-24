using Pricing.Entities;

namespace Pricing.Application.Interfaces.Markup;

public interface IMarkupContainer
{
    bool Initialized { get; }

    Models.Markup DefaultMarkup { get; }
    int DefaultCurrencyId { get; }
    Models.Markup? GetForDefaultOrNull(decimal value);
    Models.Markup? GetForCurrencyOrNull(int currencyId, decimal value);
    string CurrentVersion { get; }

    void Initialize(
        int defaultCurrencyId,
        Models.Markup defaultMarkup,
        IEnumerable<MarkupRange> @default,
        Dictionary<int, IEnumerable<MarkupRange>> other);
}
