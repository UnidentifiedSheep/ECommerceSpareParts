using Contracts.Models.Supplier;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.ProductEnrichment;

public class ImportSupplierProductTests(CombinedContainerFixture fixture)
    : IntegrationTest(fixture)
{
    [Fact]
    public async Task Import_DuplicateProductsInBatch_MergesNames()
    {
        var first = CreateProduct("ABC-123", "Bosch", ["Filter"]);
        var second = CreateProduct("ABC123", "Bosch", ["Oil filter", "filter"]);

        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [first, second]));

        var products = await GetProducts();
        products.Should().ContainSingle();
        products[0].Producer.Should().Be("Bosch");
        products[0].Names.Select(x => x.Name)
            .Should().BeEquivalentTo("Filter", "Oil filter");
    }

    [Fact]
    public async Task Import_BrandWithOuterWhitespace_MatchesPersistedValue()
    {
        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [CreateProduct("ABC-123", "Bosch", ["Filter"])]));

        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [CreateProduct("ABC123", "  Bosch  ", ["Oil filter"])]));

        var products = await GetProducts();
        products.Should().ContainSingle();
        products[0].Producer.Should().Be("Bosch");
        products[0].Names.Select(x => x.Name)
            .Should().BeEquivalentTo("Filter", "Oil filter");
    }

    [Fact]
    public async Task Import_DifferentSupplierBrandSpellings_RemainDistinct()
    {
        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [
                CreateProduct("ABC-123", "Bosch", ["First"]),
                CreateProduct("ABC-123", "BOSCH", ["Second"])
            ]));

        var products = await GetProducts();
        products.Should().HaveCount(2);
        products.Select(x => x.Producer)
            .Should().BeEquivalentTo("Bosch", "BOSCH");
    }

    [Fact]
    public async Task Import_InvalidSku_SkipsProduct()
    {
        var act = () => Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [CreateProduct(" ", "Bosch", ["Filter"])]));

        await act.Should().NotThrowAsync();
        (await GetProducts()).Should().BeEmpty();
    }

    [Fact]
    public async Task Import_Analogues_ImportsProductsNamesAndCrosses()
    {
        var analogue = CreateProduct("ANALOGUE-1", "MANN", ["Analogue name"]);
        var product = CreateProduct(
            "PRODUCT-1",
            "Bosch",
            ["Product name"],
            [analogue]);

        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [product]));

        var products = await GetProducts();
        products.Should().HaveCount(2);
        products.Single(x => x.Producer == "Bosch").Names
            .Select(x => x.Name)
            .Should().ContainSingle("Product name");
        products.Single(x => x.Producer == "MANN").Names
            .Select(x => x.Name)
            .Should().ContainSingle("Analogue name");

        var cross = await Context.SupplierProductCrosses
            .AsNoTracking()
            .SingleAsync();
        cross.LeftId.Should().Be(Math.Min(products[0].Id, products[1].Id));
        cross.RightId.Should().Be(Math.Max(products[0].Id, products[1].Id));
    }

    [Fact]
    public async Task Import_ExistingAnalogue_AddsNamesWithoutDuplicatingCross()
    {
        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [CreateProduct(
                "PRODUCT-1",
                "Bosch",
                ["Product name"],
                [CreateProduct("ANALOGUE-1", "MANN", ["First analogue name"])])]));

        await Mediator.Send(new ImportSupplierProductCommand(
            Supplier.Armtek,
            [CreateProduct(
                "PRODUCT1",
                "Bosch",
                ["Product name"],
                [CreateProduct("ANALOGUE1", "MANN", ["Second analogue name"])])]));

        var products = await GetProducts();
        products.Should().HaveCount(2);
        products.Single(x => x.Producer == "MANN").Names
            .Select(x => x.Name)
            .Should().BeEquivalentTo("First analogue name", "Second analogue name");

        var crossesCount = await Context.SupplierProductCrosses
            .AsNoTracking()
            .CountAsync();
        crossesCount.Should().Be(1);
    }

    private async Task<List<Main.Entities.Product.Enrichment.SupplierProduct>> GetProducts()
    {
        return await Context.SupplierProducts
            .AsNoTracking()
            .Include(x => x.Names)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    private static ContractSupplierProductDto CreateProduct(
        string number,
        string brand,
        IReadOnlyList<string> names,
        IReadOnlyList<ContractSupplierProductDto>? analogues = null)
    {
        return new ContractSupplierProductDto
        {
            Id = Guid.NewGuid().ToString(),
            Number = number,
            Brand = brand,
            Names = names,
            Analogues = analogues ?? [],
            Positions = []
        };
    }
}
