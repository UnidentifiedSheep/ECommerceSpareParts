using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class ProductPriceCalculator(
    ISupplierPricePolicy supplierPolicy,
    IInternalPricePolicy internalPolicy,
    IOfferScorer offerScorer,
    IMarketInfoFactory marketInfoFactory) : IProductPriceCalculator
{
    public async Task<IReadOnlyCollection<CalculatedScoredPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        CancellationToken ct)
    {
        var supplierCandidates = candidates
            .Where(x => x.SourceType == PriceOfferSourceType.Supplier)
            .ToArray();

        var supplierCalculated = await supplierPolicy.CalculateAsync(
            supplierCandidates,
            MarketInfo.Empty,
            ct);

        var market = await marketInfoFactory.CreateFromSupplierPrices(supplierCalculated);

        var internalCandidates = candidates
            .Where(x => x.SourceType == PriceOfferSourceType.OurWarehouse)
            .ToArray();

        var internalCalculated = await internalPolicy.CalculateAsync(
            internalCandidates,
            market,
            ct);

        var all = supplierCalculated
            .Concat(internalCalculated)
            .ToList();

        return await offerScorer.GetResultingScoreAsync(all, ct);
    }
}