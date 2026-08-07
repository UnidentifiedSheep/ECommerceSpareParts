using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Handlers.PriceApplier;
using Pricing.Application.Handlers.PriceApplier.GetPriceAppliers;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Entities.Exceptions;
using Pricing.Enums;
using Pricing.Integration.Tests.DataBuilders.PriceAppliers;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.PriceAppliers;

public class GetPriceAppliersTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task ConfigurationSnapshot_IncludesRegistryOnlyLocalAppliers()
    {
        var provider = Scope.ServiceProvider
            .GetRequiredService<IPriceApplierProvider>();

        var configuration = await provider.GetConfigurationAsync();

        var markup = configuration.Appliers.Single(x =>
            x.SystemName == nameof(MarkupApplier));
        markup.IsDynamic.Should().BeFalse();
        markup.States.Should().HaveCount(2);
        markup.States.Should().OnlyContain(x =>
            x.Enabled && x.Order == 0);
        markup.States.Select(x => x.Usage).Should().BeEquivalentTo(
            [
                PriceOfferSourceType.Supplier,
                PriceOfferSourceType.OurWarehouse
            ]);
        configuration.Version.Should().HaveLength(64);
    }

    [Fact]
    public async Task BySystemName_ReturnsRegistryOnlyLocalApplier()
    {
        var result = await Mediator.Send(
            new GetPriceApplierQuery(nameof(PriceRoundingApplier)));

        result.Applier.SystemName.Should().Be(nameof(PriceRoundingApplier));
        result.Applier.Name.Should().NotBe(result.Applier.SystemName);
        result.Applier.IsDynamic.Should().BeFalse();
        result.Applier.States.Select(x => x.Usage).Should().BeEquivalentTo(
            [
                PriceOfferSourceType.Supplier,
                PriceOfferSourceType.OurWarehouse
            ]);
    }

    [Fact]
    public async Task BySystemName_ReturnsPersistedDynamicApplier()
    {
        var applier = await new PriceApplierDataBuilder(Faker)
            .WithName("Requested dynamic applier")
            .WithState(PriceOfferSourceType.Supplier, 50, false)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetPriceApplierQuery(applier.SystemName));

        result.Applier.SystemName.Should().Be(applier.SystemName);
        result.Applier.Name.Should().Be("Requested dynamic applier");
        result.Applier.IsDynamic.Should().BeTrue();
        result.Applier.States.Should().ContainSingle(x =>
            x.Usage == PriceOfferSourceType.Supplier
            && x.Order == 50
            && !x.Enabled);
    }

    [Fact]
    public async Task BySystemName_ReturnsPersistedStateForLocalApplier()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.Supplier, 999, false)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetPriceApplierQuery(nameof(MarkupApplier)));

        result.Applier.IsDynamic.Should().BeFalse();
        result.Applier.Name.Should().NotBe(result.Applier.SystemName);
        result.Applier.States.Should().Contain(x =>
            x.Usage == PriceOfferSourceType.Supplier
            && x.Order == 0
            && !x.Enabled);
        result.Applier.States.Should().Contain(x =>
            x.Usage == PriceOfferSourceType.OurWarehouse
            && x.Order == 0
            && !x.Enabled);
    }

    [Fact]
    public async Task BySystemName_MissingApplier_ThrowsNotFoundException()
    {
        var query = new GetPriceApplierQuery(
            $"missing-{Faker.Random.Guid():N}");

        await Assert.ThrowsAsync<PriceApplierNotFoundException>(() =>
            Mediator.Send(query));
    }

    [Fact]
    public async Task ForUsage_ReturnsLocalAndDynamicAppliersIncludingDisabled()
    {
        var enabledDynamic = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 50)
            .BuildAndAddToDb(Context);
        var disabledDynamic = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 60, false)
            .BuildAndAddToDb(Context);
        var otherUsageDynamic = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.OurWarehouse, 70)
            .BuildAndAddToDb(Context);
        await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.Supplier, 0, false)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetPriceAppliersQuery(PriceOfferSourceType.Supplier));

        result.Appliers.Should().Contain(x =>
            x.SystemName == nameof(MarkupApplier)
            && x.Name != x.SystemName
            && !x.IsDynamic
            && !x.States.Single().Enabled);
        result.Appliers.Should().Contain(x =>
            x.SystemName == nameof(PriceRoundingApplier)
            && !x.IsDynamic
            && x.States.Single().Enabled);
        result.Appliers.Should().Contain(x =>
            x.SystemName == enabledDynamic.SystemName
            && x.Name == enabledDynamic.Name
            && x.IsDynamic
            && x.States.Single().Enabled);
        result.Appliers.Should().Contain(x =>
            x.SystemName == disabledDynamic.SystemName
            && x.IsDynamic
            && !x.States.Single().Enabled);
        result.Appliers.Should().NotContain(x =>
            x.SystemName == otherUsageDynamic.SystemName);
        result.Appliers.Should().OnlyContain(x =>
            x.States.Count == 1
            && x.States[0].Usage == PriceOfferSourceType.Supplier);
    }

    [Fact]
    public async Task WithPersistedLocalMissingUsage_ReturnsItDisabled()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.OurWarehouse, 0)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetPriceAppliersQuery(PriceOfferSourceType.Supplier));

        var markup = result.Appliers.Single(x =>
            x.SystemName == nameof(MarkupApplier));
        markup.States.Should().ContainSingle();
        markup.States[0].Usage.Should().Be(PriceOfferSourceType.Supplier);
        markup.States[0].Order.Should().Be(0);
        markup.States[0].Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task WithInvalidUsage_ThrowsValidationException()
    {
        var query = new GetPriceAppliersQuery((PriceOfferSourceType)int.MaxValue);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
    }
}
