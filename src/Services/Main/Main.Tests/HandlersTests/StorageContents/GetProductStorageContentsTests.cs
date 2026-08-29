using Abstractions.Models;
using Main.Application.Handlers.StorageContents.GetContents;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class GetProductStorageContentsTests : IntegrationTest
{
    public GetProductStorageContentsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<CurrencyRatesTestContext>();
        RegisterBasicContext<StorageTestContext>();
    }

    [Fact]
    public async Task GetProductStorageContents_ForMultipleProducts_PaginatesEachProductIndependently()
    {
        var products = GetContext<ProductTestContext>().Products.Take(2).ToArray();
        var storage = GetContext<StorageTestContext>().Storages.First();
        var currency = GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0];
        var expected = new Dictionary<int, IReadOnlyList<int>>();

        foreach (var product in products)
        {
            var contents = await new StorageContentBuilder(Faker)
                .WithProductIds(product.Id)
                .WithStorageCode(storage.Code)
                .WithCurrencyId(currency.Id)
                .WithCount(1)
                .BuildManyAndAddToDb(Context, 5);
            expected[product.Id] = contents
                .OrderBy(x => x.Id)
                .Skip(2)
                .Take(2)
                .Select(x => x.Id)
                .ToArray();
        }

        var items = products
            .Select(product => new GetProductStorageContentsItem(
                product.Id,
                new Pagination(1, 2),
                storage.Code,
                false))
            .ToArray();

        var result = await Mediator.Send(new GetProductStorageContentsQuery(items));

        foreach (var item in items)
            Assert.Equal(
                expected[item.ProductId],
                result.Content[item].Select(x => x.Id));
    }
}
