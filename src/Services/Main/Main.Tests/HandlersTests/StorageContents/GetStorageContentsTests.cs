using Abstractions.Models;
using Exceptions;
using Main.Application.Handlers.StorageContents.GetStorageContents;
using Main.Entities.Storage;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public sealed class GetStorageContentsTests : IntegrationTest
{
    public GetStorageContentsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<CurrencyRatesTestContext>();
        RegisterBasicContext<StorageTestContext>();
    }

    [Fact]
    public async Task GetStorageContents_WithProductAndStorage_FiltersContent()
    {
        var products = GetContext<ProductTestContext>().Products.Take(2).ToArray();
        var storages = GetContext<StorageTestContext>().Storages.Take(2).ToArray();
        var currencyId = GetCurrencyId();
        var expected = await CreateContent(products[0].Id, storages[0].Code, currencyId, 5);
        await CreateContent(products[0].Id, storages[0].Code, currencyId, 0);
        await CreateContent(products[0].Id, storages[1].Code, currencyId, 7);
        await CreateContent(products[1].Id, storages[0].Code, currencyId, 9);

        var result = await Mediator.Send(
            new GetStorageContentsQuery(
                products[0].Id,
                storages[0].Code,
                [],
                new Pagination(0, 20),
                false));

        Assert.Equal([expected.Id], result.Content.Select(x => x.Id));
    }

    [Fact]
    public async Task GetStorageContents_WithSortAndPagination_ReturnsRequestedPage()
    {
        var productId = GetContext<ProductTestContext>().Products.First().Id;
        var storageCode = GetContext<StorageTestContext>().Storages.First().Code;
        var currencyId = GetCurrencyId();
        await CreateContent(productId, storageCode, currencyId, 10);
        var highest = await CreateContent(productId, storageCode, currencyId, 30);
        var middle = await CreateContent(productId, storageCode, currencyId, 20);

        var firstPage = await Mediator.Send(
            new GetStorageContentsQuery(
                productId,
                storageCode,
                ["count_desc"],
                new Pagination(0, 2),
                true));
        var secondPage = await Mediator.Send(
            new GetStorageContentsQuery(
                productId,
                storageCode,
                ["count_desc"],
                new Pagination(1, 2),
                true));

        Assert.Equal([highest.Id, middle.Id], firstPage.Content.Select(x => x.Id));
        Assert.Single(secondPage.Content);
        Assert.Equal(10, secondPage.Content[0].Count);
    }

    [Fact]
    public async Task GetStorageContents_WithShowZeroCount_ReturnsZeroContent()
    {
        var productId = GetContext<ProductTestContext>().Products.First().Id;
        var storageCode = GetContext<StorageTestContext>().Storages.First().Code;
        var zeroContent = await CreateContent(productId, storageCode, GetCurrencyId(), 0);

        var result = await Mediator.Send(
            new GetStorageContentsQuery(
                productId,
                storageCode,
                [],
                new Pagination(0, 20),
                true));

        Assert.Contains(result.Content, x => x.Id == zeroContent.Id && x.Count == 0);
    }

    [Theory]
    [InlineData("unknown_desc")]
    [InlineData("count_down")]
    public async Task GetStorageContents_WithInvalidSort_ThrowsInvalidInputException(string sortBy)
    {
        var query = new GetStorageContentsQuery(
            null,
            null,
            [sortBy],
            new Pagination(0, 20),
            true);

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(query));
    }

    [Theory]
    [InlineData(-1, 20)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task GetStorageContents_WithInvalidPagination_ThrowsValidationException(
        int page,
        int size)
    {
        var query = new GetStorageContentsQuery(
            null,
            null,
            [],
            new Pagination(page, size),
            true);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
    }

    private int GetCurrencyId()
        => GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0].Id;

    private async Task<StorageContent> CreateContent(
        int productId,
        string storageCode,
        int currencyId,
        int count)
    {
        return await new StorageContentBuilder(Faker)
            .WithProductIds(productId)
            .WithStorageCode(storageCode)
            .WithCurrencyId(currencyId)
            .WithCount(count)
            .BuildAndAddToDb(Context);
    }
}
