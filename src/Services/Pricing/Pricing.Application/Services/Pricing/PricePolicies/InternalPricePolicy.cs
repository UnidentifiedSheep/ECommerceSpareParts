using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies;

public class InternalPricePolicy(
    IEnumerable<IInternalPriceApplier> appliers
) : IInternalPricePolicy
{
    public PriceOfferSourceType SourceType => PriceOfferSourceType.OurWarehouse;

    public async Task<IReadOnlyCollection<CalculatedPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        MarketInfo market,
        CancellationToken ct)
    {
        var orderedAppliers = appliers
            .OrderBy(x => x.Order)
            .ToArray();

        var result = new List<CalculatedPriceCandidate>(candidates.Count);

        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var state = PriceCalculationState.Initial(candidate, market);

            foreach (var applier in orderedAppliers)
                state = await applier.ApplyAsync(state, ct);
            

            result.Add(new CalculatedPriceCandidate(
                candidate.PriceOfferId,
                candidate.ProductId,
                candidate.TargetStorageName,
                candidate.SourceType,
                candidate.CostInBaseCurrency,
                state.SalePriceInBaseCurrency,
                state.Markup?.Proportion ?? 0,
                candidate.AvailableQuantity,
                candidate.Fulfillment.DeliveryTime,
                candidate.Fulfillment.GuaranteedDeliveryTime,
                candidate.Fulfillment.DeliveryProbability));
        }

        return result;
    }
}