using System.Text.Json;
using Json.Logic;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;

public static class DynamicApplierDslValidator
{
    public static async ValueTask<bool> IsValidAsync(
        string dslLogic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var applier = new DynamicApplierNamedObject(
                "validation",
                0,
                dslLogic);

            foreach (var state in GetValidationStates())
                await applier.ApplyAsync(state, cancellationToken);

            return true;
        }
        catch (Exception exception) when (exception is
                   JsonException or
                   JsonLogicException or
                   InvalidOperationException or
                   ArgumentException or
                   FormatException or
                   OverflowException)
        {
            return false;
        }
    }

    private static IEnumerable<PriceCalculationState> GetValidationStates()
    {
        var candidate = new PriceCandidate(
            PriceOfferId: Guid.Empty,
            ProductId: 1,
            TargetStorageName: "validation-storage",
            SourceType: PriceOfferSourceType.Supplier,
            Cost: 100m,
            CurrencyId: 1,
            AvailableQuantity: 1,
            Fulfillment: FulfillmentRouteInfo.SameStorage("validation-storage"));

        yield return PriceCalculationState.Initial(candidate, MarketInfo.Empty);
        yield return PriceCalculationState.Initial(
            candidate,
            new MarketInfo([
                new MarketInfoItem
                {
                    CostInBaseCurrency = 120m,
                    DeliveryTime = TimeSpan.FromDays(1),
                    Score = 1m
                }
            ])
            {
                OfferCount = 1,
                AvailableQuantity = 1
            }) with
        {
            BaseMarkup = MarkupResult.FromProportion(100m, 0.2m),
            AppliedRules = [new AppliedPriceRule("validation", 100m, 120m)]
        };
    }
}
