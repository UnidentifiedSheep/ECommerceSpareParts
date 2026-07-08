using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Pricing;

public interface IOfferScorer
{
    ValueTask<decimal> GetScoreAsync(
        OfferScoreContext context,
        CancellationToken cancellationToken = default);
}