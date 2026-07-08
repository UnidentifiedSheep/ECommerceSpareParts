using Pricing.Application.Models.Pricing;

namespace Pricing.Application.Interfaces.Pricing;

public interface IProductPriceCalculator
{
    Task<IReadOnlyCollection<CalculatedPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        CancellationToken ct);
}