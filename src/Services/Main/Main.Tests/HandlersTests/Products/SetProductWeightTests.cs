using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProductWeight.SetProductWeight;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts;

namespace Tests.HandlersTests.Products;

public class SetProductWeightTests : IntegrationTest
{
    public SetProductWeightTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task SetProductWeight_WhenWeightDoesNotExist_CreatesWeight()
    {
        var product = TestContext.Products[0];
        var command = new SetProductWeightCommand(
            product.Id,
            12.34m,
            WeightUnit.Gram);

        await Mediator.Send(command);

        var weight = await Context.ProductWeights
            .AsNoTracking()
            .SingleAsync(x => x.ProductId == product.Id);

        weight.Weight.Should().Be(command.Weight);
        weight.Unit.Should().Be(command.Unit);
    }

    [Fact]
    public async Task SetProductWeight_WhenWeightExists_UpdatesWeight()
    {
        var product = TestContext.Products[1];
        await new ProductWeightBuilder(Faker)
            .WithProductId(product.Id)
            .WithWeight(2m)
            .WithUnit(WeightUnit.Kilogram)
            .BuildAndAddToDb(Context);

        var command = new SetProductWeightCommand(
            product.Id,
            750m,
            WeightUnit.Gram);

        await Mediator.Send(command);

        var weights = await Context.ProductWeights
            .AsNoTracking()
            .Where(x => x.ProductId == product.Id)
            .ToListAsync();

        weights.Should().HaveCount(1);
        weights[0].Weight.Should().Be(command.Weight);
        weights[0].Unit.Should().Be(command.Unit);
    }
}