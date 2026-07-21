using Application.Common.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities.Offers;
using Pricing.Enums;
using Pricing.Integration.Tests.DataBuilders.Offers;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.Pricing;

public class CalculateCandidatesHandlerTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    private const int ProductId = 100;
    private const string StorageName = "main";

    [Fact]
    public async Task Handle_WithCalculatedCandidate_PersistsAllConfigurationVersions()
    {
        var offer = await CreateOffer();
        var settingsVersion = Guid.NewGuid();
        var handler = CreateHandler(Result(
            "markup-v2",
            "appliers-v3",
            settingsVersion,
            [ScoredCandidate(offer)]));

        await handler.Handle(
            new CalculateCandidatesCommand(ProductId, StorageName),
            CancellationToken.None);

        var option = await Context.Set<ProductPriceOption>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceOfferId == offer.Id);
        option.MarkupVersion.Should().Be("markup-v2");
        option.AppliersVersion.Should().Be("appliers-v3");
        option.PricingSettingsVersion.Should().Be(settingsVersion);
    }

    [Fact]
    public async Task Handle_WithExistingOption_UpdatesVersionsThroughUpsert()
    {
        var offer = await CreateOffer();
        await new ProductPriceOptionDataBuilder(Faker)
            .WithPriceOfferId(offer.Id)
            .WithVersions("markup-v1", "appliers-v1", Guid.NewGuid())
            .BuildAndAddToDb(Context);
        var updatedSettingsVersion = Guid.NewGuid();
        var handler = CreateHandler(Result(
            "markup-v2",
            "appliers-v2",
            updatedSettingsVersion,
            [ScoredCandidate(offer)]));

        await handler.Handle(
            new CalculateCandidatesCommand(ProductId, StorageName),
            CancellationToken.None);

        var option = await Context.Set<ProductPriceOption>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceOfferId == offer.Id);
        option.MarkupVersion.Should().Be("markup-v2");
        option.AppliersVersion.Should().Be("appliers-v2");
        option.PricingSettingsVersion.Should().Be(updatedSettingsVersion);
    }

    [Fact]
    public async Task Handle_WithNoCalculatedCandidates_DoesNotDeleteExistingOptions()
    {
        var offer = await CreateOffer();
        var existingSettingsVersion = Guid.NewGuid();
        await new ProductPriceOptionDataBuilder(Faker)
            .WithPriceOfferId(offer.Id)
            .WithVersions("markup-v1", "appliers-v1", existingSettingsVersion)
            .BuildAndAddToDb(Context);
        var handler = CreateHandler(Result(
            "markup-v2",
            "appliers-v2",
            Guid.NewGuid(),
            []));

        await handler.Handle(
            new CalculateCandidatesCommand(ProductId, StorageName),
            CancellationToken.None);

        var option = await Context.Set<ProductPriceOption>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceOfferId == offer.Id);
        option.MarkupVersion.Should().Be("markup-v1");
        option.AppliersVersion.Should().Be("appliers-v1");
        option.PricingSettingsVersion.Should().Be(existingSettingsVersion);
    }

    private CalculateCandidatesHandler CreateHandler(ProductPriceCalculationResult result)
    {
        var builder = new Mock<IPriceCandidateBuilder>();
        builder
            .Setup(x => x.Build(
                It.IsAny<IReadOnlyCollection<PriceOffer>>(),
                StorageName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var calculator = new Mock<IProductPriceCalculator>();
        calculator
            .Setup(x => x.CalculateAsync(
                It.IsAny<IReadOnlyCollection<PriceCandidate>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return new CalculateCandidatesHandler(
            Scope.ServiceProvider.GetRequiredService<IReadRepository<PriceOffer, Guid>>(),
            calculator.Object,
            builder.Object,
            Scope.ServiceProvider.GetRequiredService<IProductPriceOptionRepository>());
    }

    private Task<PriceOffer> CreateOffer()
    {
        return new PriceOfferDataBuilder(Faker)
            .WithProductId(ProductId)
            .WithStorageName(StorageName)
            .BuildAndAddToDb(Context);
    }

    private static ProductPriceCalculationResult Result(
        string markupVersion,
        string appliersVersion,
        Guid settingsVersion,
        IReadOnlyCollection<CalculatedScoredPriceCandidate> candidates)
    {
        return new ProductPriceCalculationResult
        {
            MarkupVersion = markupVersion,
            AppliersVersion = appliersVersion,
            PricingSettingsVersion = settingsVersion,
            Candidates = candidates
        };
    }

    private static CalculatedScoredPriceCandidate ScoredCandidate(PriceOffer offer)
    {
        return new CalculatedScoredPriceCandidate
        {
            PriceOfferId = offer.Id,
            ProductId = offer.ProductId,
            StorageName = offer.OfferForStorage,
            SourceType = PriceOfferSourceType.Supplier,
            CurrencyId = offer.CurrencyId,
            Cost = offer.PurchasePrice,
            Price = offer.PurchasePrice * 1.2m,
            Markup = 0.2m,
            AvailableQuantity = offer.AvailableQuantity,
            DeliveryTime = TimeSpan.FromDays(1),
            GuaranteedDeliveryTime = TimeSpan.FromDays(2),
            DeliveryProbability = offer.DeliveryProbability,
            Score = 10m
        };
    }
}
