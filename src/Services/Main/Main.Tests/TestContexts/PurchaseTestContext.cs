using Main.Entities;
using Main.Entities.Product;
using Main.Persistence.Context;
using MediatR;
using Tests.MockData.ScenatioExtensions;
using Tests.MockData.SeedExtensions;

namespace Tests.TestContexts;

public class PurchaseTestContext(DContext ctx, IMediator mediator) : SystemUserTestContext(ctx, mediator)
{
    public User User { get; private set; } = null!;
    public User Supplier { get; private set; } = null!;
    public User Carrier { get; private set; } = null!;
    public Currency Currency { get; private set; } = null!;
    public Storage StorageTo { get; private set; } = null!;
    public Storage StorageFrom { get; private set; } = null!;
    public List<Product> Articles { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var users = await DbContext.CreateUsers(3);

        User = users[0];
        Supplier = users[1];
        Carrier = users[2];

        Currency = (await DbContext.CreateCurrencies())[0];

        var storages = await DbContext.CreateStorages(2);
        var storageNames = storages.Select(x => x.Name).ToList();
        StorageTo = storages[0];
        StorageFrom = storages[1];

        (_, Articles) = await DbContext.CreateProducerAndArticles(5, 10);
        await DbContext.AddStorageContentsAndIncreaseArticleCounts(
            3,
            [Currency.Id],
            Articles.Select(x => x.Id),
            storageNames);

        await DbContext.AddStorageToUser(Supplier, StorageFrom);
        await DbContext.CreateStorageRoutes(
            StorageFrom.Name,
            StorageTo.Name,
            Carrier.Id,
            [Currency.Id]);

        await base.InitializeAsync(cancellationToken);
    }
}