using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Storage;

namespace Tests.TestContexts;

public class StorageContentTestContext(
    DContext ctx, 
    IMediator mediator,
    StorageTestContext storage,
    ProductTestContext product,
    CurrencyTestContext currency) 
    : TestContextBase<DContext>(ctx, mediator), ITestContextRegistrator
{
    public IReadOnlyCollection<StorageContent> StorageContents { get; private set; } = null!;
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        StorageContents = await new StorageContentBuilder(Faker)
            .WithCurrencyId(currency.Currencies.First().Id)
            .WithProducts(product.Products)
            .WithStorageName(storage.Storages.First(x => x.Type == StorageType.Warehouse).Name)
            .BuildManyAndAddToDb(DbContext, 10);
    }

    public static void Register(ITest test)
    {
        test.RegisterBasicContext<CurrencyTestContext>();
        test.RegisterBasicContext<ProductTestContext>();
        test.RegisterBasicContext<StorageTestContext>();
        test.RegisterBasicContext<StorageContentTestContext>();
    }
}