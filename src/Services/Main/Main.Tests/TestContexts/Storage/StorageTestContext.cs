using Main.Enums;
using Main.Persistence.Context;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.Storage;

namespace Tests.TestContexts.Storage;

public class StorageTestContext(DContext ctx) : TestContextBase<DContext>(ctx)
{
    public IReadOnlyCollection<Main.Entities.Storage.Storage> Storages { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Storages = await BuilderExtensions.BuildManyCombinedAndAddToDb(
            DbContext,
            2,
            new StorageBuilder(Faker).WithType(StorageType.Warehouse),
            new StorageBuilder(Faker).WithType(StorageType.SupplierStorage));
    }
}