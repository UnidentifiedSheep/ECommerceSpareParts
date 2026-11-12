using Main.Enums;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Storage;
using Tests.Extensions;

namespace Tests.TestContexts.Storage;

public class StorageTestContext(UnifiedDbContext ctx) : TestContextBase<UnifiedDbContext>(ctx)
{
	public IReadOnlyCollection<Main.Entities.Storage.Storage> Storages { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
		=> Storages = await BuilderExtensions.BuildManyCombinedAndAddToDb(
			DbContext,
			2,
			true,
			new StorageBuilder(Faker).WithType(StorageType.Warehouse),
			new StorageBuilder(Faker).WithType(StorageType.SupplierStorage));
}
