using Application.Common.Interfaces.Settings;
using FluentAssertions;
using Moq;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PricePolicy;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Application.Services.Pricing;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Integration.Tests.Services.Pricing;

public class ProductPriceCalculatorTests
{
    private const string MarkupVersion = "markup-v2";
    private const string AppliersVersion = "appliers-v3";
    private readonly Guid _settingsVersion = Guid.NewGuid();
    private readonly Mock<ISupplierPricePolicy> _supplierPolicy = new();
    private readonly Mock<IInternalPricePolicy> _internalPolicy = new();
    private readonly Mock<IOfferScorer> _offerScorer = new();
    private readonly Mock<IMarketInfoFactory> _marketInfoFactory = new();
    private readonly ProductPriceCalculator _calculator;

    public ProductPriceCalculatorTests()
    {
        var markupContainer = new Mock<IMarkupContainer>();
        markupContainer.SetupGet(x => x.CurrentVersion).Returns(MarkupVersion);

        var priceApplierService = new Mock<IPriceApplierService>();
        priceApplierService
            .Setup(x => x.GetCurrentConfigurationVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(AppliersVersion);

        var settingsService = new Mock<ISettingsService>();
        settingsService
            .Setup(x => x.GetOrDefault<PricingSetting>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PricingSetting(new PricingSettingData
            {
                Version = _settingsVersion
            }));

        _marketInfoFactory
            .Setup(x => x.CreateFromSupplierPrices(
                It.IsAny<IReadOnlyCollection<CalculatedPriceCandidate>>()))
            .ReturnsAsync(MarketInfo.Empty);

        _calculator = new ProductPriceCalculator(
            _supplierPolicy.Object,
            _internalPolicy.Object,
            _offerScorer.Object,
            markupContainer.Object,
            priceApplierService.Object,
            _marketInfoFactory.Object,
            settingsService.Object);
    }

    [Fact]
    public async Task CalculateAsync_ReturnsVersionsUsedForCalculation()
    {
        var supplier = Candidate(PriceOfferSourceType.Supplier);
        var internalCandidate = Candidate(PriceOfferSourceType.OurWarehouse);
        var supplierCalculated = Calculated(supplier);
        var internalCalculated = Calculated(internalCandidate);
        IReadOnlyList<CalculatedScoredPriceCandidate> scored =
        [
            CalculatedScoredPriceCandidate.From(supplierCalculated, 10m),
            CalculatedScoredPriceCandidate.From(internalCalculated, 20m)
        ];

        _supplierPolicy
            .Setup(x => x.CalculateAsync(
                It.Is<IReadOnlyCollection<PriceCandidate>>(items => items.Single() == supplier),
                MarketInfo.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([supplierCalculated]);
        _internalPolicy
            .Setup(x => x.CalculateAsync(
                It.Is<IReadOnlyCollection<PriceCandidate>>(items => items.Single() == internalCandidate),
                MarketInfo.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([internalCalculated]);
        _offerScorer
            .Setup(x => x.GetResultingScoreAsync(
                It.IsAny<IReadOnlyList<CalculatedPriceCandidate>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<IReadOnlyList<CalculatedScoredPriceCandidate>>(scored));

        var result = await _calculator.CalculateAsync(
            [supplier, internalCandidate],
            CancellationToken.None);

        result.MarkupVersion.Should().Be(MarkupVersion);
        result.AppliersVersion.Should().Be(AppliersVersion);
        result.PricingSettingsVersion.Should().Be(_settingsVersion);
        result.Candidates.Should().BeEquivalentTo(scored);
    }

    [Fact]
    public async Task CalculateAsync_WithNoCandidates_ReturnsVersionsAndEmptyCandidates()
    {
        _supplierPolicy
            .Setup(x => x.CalculateAsync(
                It.IsAny<IReadOnlyCollection<PriceCandidate>>(),
                MarketInfo.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _internalPolicy
            .Setup(x => x.CalculateAsync(
                It.IsAny<IReadOnlyCollection<PriceCandidate>>(),
                MarketInfo.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _offerScorer
            .Setup(x => x.GetResultingScoreAsync(
                It.IsAny<IReadOnlyList<CalculatedPriceCandidate>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<IReadOnlyList<CalculatedScoredPriceCandidate>>([]));

        var result = await _calculator.CalculateAsync([], CancellationToken.None);

        result.MarkupVersion.Should().Be(MarkupVersion);
        result.AppliersVersion.Should().Be(AppliersVersion);
        result.PricingSettingsVersion.Should().Be(_settingsVersion);
        result.Candidates.Should().BeEmpty();
    }

    private static PriceCandidate Candidate(PriceOfferSourceType sourceType)
    {
        return new PriceCandidate(
            Guid.NewGuid(),
            10,
            "main",
            sourceType,
            100m,
            1,
            10,
            FulfillmentRouteInfo.SameStorage("main"));
    }

    private static CalculatedPriceCandidate Calculated(PriceCandidate candidate)
    {
        return new CalculatedPriceCandidate
        {
            PriceOfferId = candidate.PriceOfferId,
            ProductId = candidate.ProductId,
            StorageName = candidate.TargetStorageName,
            SourceType = candidate.SourceType,
            CurrencyId = candidate.CurrencyId,
            Cost = candidate.Cost,
            Price = 120m,
            Markup = 0.2m,
            AvailableQuantity = candidate.AvailableQuantity,
            DeliveryTime = TimeSpan.Zero,
            GuaranteedDeliveryTime = TimeSpan.Zero,
            DeliveryProbability = 100
        };
    }
}
