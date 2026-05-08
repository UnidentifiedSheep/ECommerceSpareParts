using Main.Entities.Storage;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders.Storage;

namespace Tests.TestContexts;

public class StorageTestContext(DContext ctx, IMediator mediator) : TestContextBase<DContext>(ctx, mediator)
{
    public IReadOnlyCollection<Storage> Storages { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Storages = await BuilderExtensions.BuildManyCombinedAndAddToDb(
            DbContext,
            2,
            new StorageBuilder(Faker).WithType(StorageType.Warehouse),
            new StorageBuilder(Faker).WithType(StorageType.SupplierStorage));
    }
}