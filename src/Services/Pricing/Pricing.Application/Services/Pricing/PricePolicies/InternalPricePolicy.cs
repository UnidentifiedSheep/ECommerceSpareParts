using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies;

public class InternalPricePolicy(
    IPriceApplierService applierService
) : IInternalPricePolicy
{
    public PriceOfferSourceType SourceType => PriceOfferSourceType.OurWarehouse;

    public async Task<IReadOnlyCollection<CalculatedPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        MarketInfo market,
        CancellationToken ct)
    {
        var orderedAppliers = await applierService.GetPriceAppliersAsync(
            SourceType, 
            ct);

        var result = new List<CalculatedPriceCandidate>(candidates.Count);

        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var state = PriceCalculationState.Initial(candidate, market);

            foreach (var applier in orderedAppliers)
                state = await applier.ApplyAsync(state, ct);
            

            result.Add(new CalculatedPriceCandidate
            {
                AvailableQuantity = candidate.AvailableQuantity,
                DeliveryTime = candidate.Fulfillment.DeliveryTime,
                DeliveryProbability = candidate.Fulfillment.DeliveryProbability,
                GuaranteedDeliveryTime = candidate.Fulfillment.GuaranteedDeliveryTime,
                Markup = state.BaseMarkup?.Proportion ?? 0,
                Price = state.SalePrice,
                CurrencyId = candidate.CurrencyId,
                ProductId = candidate.ProductId,
                PriceOfferId = candidate.PriceOfferId,
                SourceType = candidate.SourceType,
                StorageName = candidate.TargetStorageName,
                Cost = candidate.Cost,
            });
        }

        return result;
    }
}