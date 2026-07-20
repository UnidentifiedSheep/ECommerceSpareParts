using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services.Pricing;

public sealed class ProductPriceCalculator(
    ISupplierPricePolicy supplierPolicy,
    IInternalPricePolicy internalPolicy,
    IOfferScorer offerScorer,
    IMarkupContainer markupContainer,
    IPriceApplierService priceApplierService,
    IMarketInfoFactory marketInfoFactory,
    ISettingsService settingsService) : IProductPriceCalculator
{
    public async Task<ProductPriceCalculationResult> CalculateAsync(
        IReadOnlyCollection<PriceCandidate> candidates,
        CancellationToken ct)
    {
        var appliersVersion = await priceApplierService
            .GetCurrentConfigurationVersionAsync(ct);
        var pricingSettingsVersion = (await settingsService
            .GetOrDefault<PricingSetting>(ct)).Data.Version;

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

        return new ProductPriceCalculationResult
        {
            Candidates = await offerScorer.GetResultingScoreAsync(all, ct),
            MarkupVersion = markupContainer.CurrentVersion,
            AppliersVersion = appliersVersion,
            PricingSettingsVersion = pricingSettingsVersion
        };
    }
}
