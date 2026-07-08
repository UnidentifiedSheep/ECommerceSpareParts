using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies;

public class SupplierPricePolicy(
    IEnumerable<ISupplierPriceApplier> appliers
    ) : ISupplierPricePolicy
{
    public PriceOfferSourceType SourceType => PriceOfferSourceType.Supplier;
    public async Task<IReadOnlyCollection<CalculatedPriceCandidate>> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        MarketInfo market,
        CancellationToken ct)
    {
        var result = new List<CalculatedPriceCandidate>();
        var orderedAppliers = appliers
            .OrderBy(x => x.Order)
            .ToArray();

        foreach (var candidate in candidates)
        {
            PriceCalculationState state = PriceCalculationState
                .Initial(candidate, market);
            
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