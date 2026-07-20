using Domain.CommonEntities;
using Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Handlers.PriceApplier.DeletePriceApplier;
using Pricing.Application.Interfaces.Cache;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Entities.Exceptions;
using Pricing.Entities.Pricing;
using Pricing.Enums;
using Pricing.Integration.Tests.DataBuilders.PriceAppliers;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.PriceAppliers;

public class DeletePriceApplierTests(CombinedContainerFixture fixture)
    : IntegrationTest(fixture)
{
    [Fact]
    public async Task WithDynamicApplier_DeletesApplierAndInvalidatesConfiguration()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .BuildAndAddToDb(Context);
        var provider = Scope.ServiceProvider.GetRequiredService<IPriceApplierProvider>();
        var initialConfiguration = await provider.GetConfigurationAsync();

        await Mediator.Send(new DeletePriceApplierCommand(existing.SystemName));

        var applierExists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == existing.SystemName);
        var stateExists = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .AnyAsync(x => x.PriceApplierSystemName == existing.SystemName);
        applierExists.Should().BeFalse();
        stateExists.Should().BeFalse();

        var updatedConfiguration = await provider.GetConfigurationAsync();
        updatedConfiguration.Appliers.Should().NotContain(x =>
            x.SystemName == existing.SystemName);
        updatedConfiguration.Version.Should().NotBe(initialConfiguration.Version);

        var recalculationJobExists = await Context.Set<UniqJob>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == InvalidateStalePriceOptionsLrt.LrtName);
        recalculationJobExists.Should().BeTrue();
    }

    [Fact]
    public async Task WithLocalApplier_ThrowsLocalPriceApplierCannotBeDeletedException()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.Supplier, 0, false)
            .BuildAndAddToDb(Context);

        var exception = await Assert.ThrowsAsync<LocalPriceApplierCannotBeDeletedException>(
            () => Mediator.Send(new DeletePriceApplierCommand(existing.SystemName)));

        exception.MessageKey.Should().Be("price.applier.local.cannot.be.deleted");
        var exists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == existing.SystemName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task WithUnknownApplier_ThrowsPriceApplierNotFoundException()
    {
        var systemName = $"missing-{Faker.Random.Guid():N}";

        var exception = await Assert.ThrowsAsync<PriceApplierNotFoundException>(
            () => Mediator.Send(new DeletePriceApplierCommand(systemName)));

        exception.MessageKey.Should().Be("price.applier.not.found");
    }

    [Fact]
    public async Task WithEmptySystemName_ThrowsValidationException()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => Mediator.Send(new DeletePriceApplierCommand("")));

        exception.Errors.Should().ContainSingle(x =>
            x.ErrorCode == "price.applier.system.name.required");
    }
}
