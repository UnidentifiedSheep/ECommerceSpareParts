using FluentAssertions;
using Main.Application.Handlers.Products;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public class GetProductStockTests : IntegrationTest
{
    public GetProductStockTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
    }

    private StorageContentTestContext TestContext => GetContext<StorageContentTestContext>();

    [Fact]
    public async Task GetProductStock_WithoutStorageName_ReturnsProductStock()
    {
        var content = TestContext.StorageContents.First(x => x.Count > 0);
        var expected = TestContext.StorageContents
            .Where(x => x.ProductId == content.ProductId)
            .Sum(x => x.Count);

        var result = await Mediator.Send(new GetProductStockQuery(content.ProductId, null));

        result.Stock.Should().Be(expected);
    }

    [Fact]
    public async Task GetProductStock_WithStorageName_ReturnsStorageContentStock()
    {
        var content = TestContext.StorageContents.First(x => x.Count > 0);
        var expected = TestContext.StorageContents
            .Where(x =>
                x.ProductId == content.ProductId &&
                x.StorageName == content.StorageName &&
                x.Count > 0)
            .Sum(x => x.Count);

        var result = await Mediator.Send(new GetProductStockQuery(content.ProductId, content.StorageName));

        result.Stock.Should().Be(expected);
    }

    [Fact]
    public async Task GetProductStock_WithUnknownStorageName_ReturnsZero()
    {
        var content = TestContext.StorageContents.First(x => x.Count > 0);

        var result = await Mediator.Send(new GetProductStockQuery(content.ProductId, "unknown-storage"));

        result.Stock.Should().Be(0);
    }
}
