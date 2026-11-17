using Main.Entities.Product.Stock;
using Main.Entities.Storage.Enums;
using Main.Entities.Storage.Storage;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.Interfaces;
using Tests.TestContexts.Currency;

namespace Tests.TestContexts.Storage;

public class StorageContentTestContext(
	UnifiedDbContext ctx,
	StorageTestContext storage,
	ProductTestContext product,
	CurrencyTestContext currency) : TestContextBase<UnifiedDbContext>(ctx), IDependentTestContext
{
	public IReadOnlyCollection<StorageContent> StorageContents { get; private set; } = null!;

	public static Type[] DependsOn { get; } =
	[
		typeof(CurrencyRatesTestContext), typeof(ProductTestContext), typeof(StorageTestContext)
	];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		StorageContents = await new StorageContentBuilder(Faker)
			.WithCurrencyId(currency.Currencies[0].Id)
			.WithProducts(product.Products)
			.WithStorageCode(storage.Storages.First(x => x.Type == StorageType.Warehouse).Code)
			.BuildManyAndAddToDb(DbContext, 10);

		await DbContext.AddRangeAsync(
			StorageContents
				.GroupBy(content => (content.StorageCode, content.ProductId))
				.Select(group => ProductStock.Create(
					group.Key.StorageCode,
					group.Key.ProductId,
					group.Sum(content => content.Count))),
			cancellationToken);

		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
