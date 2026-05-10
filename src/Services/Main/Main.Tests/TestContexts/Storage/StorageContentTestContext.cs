using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Storage;
using Tests.TestContexts.Currency;

namespace Tests.TestContexts.Storage;

public class StorageContentTestContext(
    DContext ctx,
    StorageTestContext storage,
    ProductTestContext product,
    CurrencyTestContext currency)
    : TestContextBase<DContext>(ctx), IDependentTestContext
{
    public IReadOnlyCollection<StorageContent> StorageContents { get; private set; } = null!;

    public static Type[] DependsOn { get; } =
    [
        typeof(CurrencyRatesTestContext),
        typeof(ProductTestContext),
        typeof(StorageTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        StorageContents = await new StorageContentBuilder(Faker)
            .WithCurrencyId(currency.Currencies[0].Id)
            .WithProducts(product.Products)
            .WithStorageName(storage.Storages.First(x => x.Type == StorageType.Warehouse).Name)
            .BuildManyAndAddToDb(DbContext, 10);

        var products = product.Products.ToDictionary(k => k.Id);

        foreach (var content in StorageContents)
            products[content.ProductId].IncreaseStock(content.Count);

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}