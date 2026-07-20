using Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Nodes;
using Pricing.Application.Dtos.PriceApplier;
using Pricing.Application.Handlers.PriceApplier;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers;
using Pricing.Application.Services.Pricing.PricePolicies.PriceAppliers.Internal;
using Pricing.Entities.Pricing;
using Pricing.Enums;
using Pricing.Integration.Tests.DataBuilders.PriceAppliers;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlerTests.PriceAppliers;

public class UpsertPriceApplierTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task WithNewDynamicApplier_CreatesApplierAndStates()
    {
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        const string name = "Dynamic price rule";
        const string dslLogic = """{"var":"salePrice"}""";
        var command = new UpsertPriceApplierCommand(
            systemName,
            name,
            dslLogic,
            [
                State(PriceOfferSourceType.Supplier, 10),
                State(PriceOfferSourceType.OurWarehouse, 20, false)
            ]);

        var result = await Mediator.Send(command);

        result.Applier.SystemName.Should().Be(systemName);
        result.Applier.Name.Should().Be(name);
        result.Applier.IsDynamic.Should().BeTrue();
        result.Applier.DslLogic.Should().Be(dslLogic);
        result.Applier.States.Should().BeEquivalentTo(
            command.States,
            options => options.ExcludingMissingMembers());

        var applier = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .Include(x => x.States)
            .SingleAsync(x => x.SystemName == systemName);

        JsonNode.DeepEquals(
                JsonNode.Parse(applier.DslLogic!),
                JsonNode.Parse(dslLogic))
            .Should()
            .BeTrue();
        applier.Name.Should().Be(name);
        applier.States.Should().BeEquivalentTo(
            command.States,
            options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task WithExistingDynamicApplier_UpdatesDslLogicAndStates()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .BuildAndAddToDb(Context);
        const string newDslLogic = """{"*":[{"var":"salePrice"},2]}""";
        var command = new UpsertPriceApplierCommand(
            existing.SystemName,
            "Updated dynamic price rule",
            newDslLogic,
            [
                State(PriceOfferSourceType.Supplier, 30, false),
                State(PriceOfferSourceType.OurWarehouse, 40)
            ]);

        await Mediator.Send(command);

        var applier = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .Include(x => x.States)
            .SingleAsync(x => x.SystemName == existing.SystemName);

        JsonNode.DeepEquals(
                JsonNode.Parse(applier.DslLogic!),
                JsonNode.Parse(newDslLogic))
            .Should()
            .BeTrue();
        applier.Name.Should().Be(command.Name);
        applier.States.Should().BeEquivalentTo(
            command.States,
            options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task WithExistingDynamicApplier_RemovesStatesMissingFromSnapshot()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .WithState(PriceOfferSourceType.OurWarehouse, 20)
            .BuildAndAddToDb(Context);
        var command = new UpsertPriceApplierCommand(
            existing.SystemName,
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.Supplier, 30)]);

        await Mediator.Send(command);

        var states = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .Where(x => x.PriceApplierSystemName == existing.SystemName)
            .ToListAsync();
        states.Should().ContainSingle(x =>
            x.Usage == PriceOfferSourceType.Supplier
            && x.Order == 30);
    }

    [Fact]
    public async Task WithNewLocalApplier_UsesLocalOrderAndIgnoresDslLogic()
    {
        var command = new UpsertPriceApplierCommand(
            nameof(MarkupApplier),
            null,
            "not-json",
            [
                State(PriceOfferSourceType.Supplier, 999),
                State(PriceOfferSourceType.OurWarehouse, null)
            ]);

        var result = await Mediator.Send(command);

        result.Applier.IsDynamic.Should().BeFalse();
        result.Applier.Name.Should().NotBeNullOrWhiteSpace();
        result.Applier.Name.Should().NotBe(result.Applier.SystemName);
        result.Applier.DslLogic.Should().BeNull();
        result.Applier.States.Should().OnlyContain(x => x.Order == 0);

        var applier = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .Include(x => x.States)
            .SingleAsync(x => x.SystemName == nameof(MarkupApplier));

        applier.DslLogic.Should().BeNull();
        applier.States.Should().OnlyContain(x => x.Order == 0);
    }

    [Fact]
    public async Task WithExistingLocalApplier_ChangesEnabledButKeepsLocalOrder()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(PriceRoundingApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.Supplier, 100_000)
            .BuildAndAddToDb(Context);
        var command = new UpsertPriceApplierCommand(
            existing.SystemName,
            null,
            null,
            [State(PriceOfferSourceType.Supplier, -1, false)]);

        await Mediator.Send(command);

        var state = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceApplierSystemName == existing.SystemName);

        state.Enabled.Should().BeFalse();
        state.Order.Should().Be(100_000);
    }

    [Fact]
    public async Task WithExistingLocalApplier_RemovesMissingStateAndDoesNotApplyIt()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .AsLocal()
            .WithState(PriceOfferSourceType.Supplier, 0)
            .WithState(PriceOfferSourceType.OurWarehouse, 0)
            .BuildAndAddToDb(Context);
        var command = new UpsertPriceApplierCommand(
            existing.SystemName,
            null,
            null,
            [State(PriceOfferSourceType.Supplier, null)]);

        await Mediator.Send(command);

        var states = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .Where(x => x.PriceApplierSystemName == existing.SystemName)
            .ToListAsync();
        states.Should().ContainSingle(x =>
            x.Usage == PriceOfferSourceType.Supplier);

        var service = Scope.ServiceProvider
            .GetRequiredService<IPriceApplierService>();
        var warehouseAppliers = await service.GetPriceAppliersAsync(
            PriceOfferSourceType.OurWarehouse);
        warehouseAppliers.Should().NotContain(x => x is MarkupApplier);
    }

    [Fact]
    public async Task WithDuplicateUsages_ThrowsValidationException()
    {
        var command = new UpsertPriceApplierCommand(
            $"dynamic-{Faker.Random.Guid():N}",
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [
                State(PriceOfferSourceType.Supplier, 10),
                State(PriceOfferSourceType.Supplier, 20)
            ]);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));

        exception.Errors.Should().ContainSingle(x =>
            x.ErrorCode == "price.applier.usage.duplicate");
    }

    [Fact]
    public async Task WithDynamicApplierWithoutDslLogic_ThrowsInvalidInputException()
    {
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            "  ",
            []);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.dsl.logic.required");
        var exists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == systemName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task WithDynamicApplierWithoutName_ThrowsInvalidInputException()
    {
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "  ",
            """{"var":"salePrice"}""",
            []);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.name.required");
        var exists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == systemName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task WithDynamicApplierNameOverMaximumLength_ThrowsValidationException()
    {
        var command = new UpsertPriceApplierCommand(
            $"dynamic-{Faker.Random.Guid():N}",
            new string('a', 129),
            """{"var":"salePrice"}""",
            []);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));

        exception.Errors.Should().ContainSingle(x =>
            x.ErrorCode == "price.applier.name.max.length");
    }

    [Theory]
    [InlineData("{")]
    [InlineData("""{"unknown":[1,2]}""")]
    [InlineData("""{"==":[1,1]}""")]
    [InlineData("""{"-":[0,1]}""")]
    public async Task WithInvalidDslLogic_ThrowsInvalidInputException(string dslLogic)
    {
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            dslLogic,
            [State(PriceOfferSourceType.Supplier, 10)]);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.dsl.logic.invalid");
        var exists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == systemName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task WithDynamicStateWithoutOrder_ThrowsInvalidInputException()
    {
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.Supplier, null)]);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.order.required");
        var exists = await Context.Set<PriceApplier>()
            .AsNoTracking()
            .AnyAsync(x => x.SystemName == systemName);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task WithUnsupportedLocalUsage_ThrowsInvalidInputException()
    {
        var command = new UpsertPriceApplierCommand(
            nameof(MinimumSupplierPriceApplier),
            null,
            null,
            [State(PriceOfferSourceType.Supplier, null)]);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.usage.not.supported");
    }

    [Fact]
    public async Task WithExistingApplierOfDifferentKind_ThrowsInvalidInputException()
    {
        var existing = await new PriceApplierDataBuilder(Faker)
            .WithSystemName(nameof(MarkupApplier))
            .BuildAndAddToDb(Context);
        var command = new UpsertPriceApplierCommand(
            existing.SystemName,
            null,
            null,
            []);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.system.name.conflict");
    }

    [Fact]
    public async Task WithOrderOccupiedByEnabledApplierForSameUsage_ThrowsInvalidInputException()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .BuildAndAddToDb(Context);
        var command = new UpsertPriceApplierCommand(
            $"dynamic-{Faker.Random.Guid():N}",
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.Supplier, 10)]);

        var exception = await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        exception.MessageKey.Should().Be("price.applier.order.duplicate");
    }

    [Fact]
    public async Task WithOrderOccupiedByEnabledApplierForDifferentUsage_Succeeds()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .BuildAndAddToDb(Context);
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.OurWarehouse, 10)]);

        await Mediator.Send(command);

        var exists = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .AnyAsync(x => x.PriceApplierSystemName == systemName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task WithOrderOccupiedOnlyByDisabledApplier_Succeeds()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10, false)
            .BuildAndAddToDb(Context);
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.Supplier, 10)]);

        await Mediator.Send(command);

        var state = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceApplierSystemName == systemName);
        state.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task WithDisabledStateUsingOccupiedOrder_Succeeds()
    {
        await new PriceApplierDataBuilder(Faker)
            .WithState(PriceOfferSourceType.Supplier, 10)
            .BuildAndAddToDb(Context);
        var systemName = $"dynamic-{Faker.Random.Guid():N}";
        var command = new UpsertPriceApplierCommand(
            systemName,
            "Dynamic price rule",
            """{"var":"salePrice"}""",
            [State(PriceOfferSourceType.Supplier, 10, false)]);

        await Mediator.Send(command);

        var state = await Context.Set<PriceApplierState>()
            .AsNoTracking()
            .SingleAsync(x => x.PriceApplierSystemName == systemName);
        state.Enabled.Should().BeFalse();
    }

    private static UpsertPriceApplierStateDto State(
        PriceOfferSourceType usage,
        int? order,
        bool enabled = true)
    {
        return new UpsertPriceApplierStateDto
        {
            Usage = usage,
            Order = order,
            Enabled = enabled
        };
    }
}
