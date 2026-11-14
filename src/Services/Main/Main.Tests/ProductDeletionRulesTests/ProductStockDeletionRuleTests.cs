using Main.Application.Services;
using Main.Entities.Product;
using Main.Entities.Storage;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.ProductDeletionRulesTests;

public class ProductStockDeletionRuleTests : ProductDeletionRuleTestsBase<ProductStockDeletionRule>
{
	public ProductStockDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageTestContext>();

	[Fact]
	public async Task ProductWithPositiveStock_IsBlocked()
	{
		var product = await CreateUnreferencedProduct();
		await AddStock(product.Id, 1);

		await ShouldBlock(product.Id, ArticleDeletionStockMessage.Key);
	}

	[Fact]
	public async Task ProductWithZeroStock_IsNotBlocked()
	{
		var product = await CreateUnreferencedProduct();
		await AddStock(product.Id, 0);

		await ShouldNotBlock(product.Id);
	}

	[Fact]
	public async Task ProductWithoutStockRows_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);

	private async Task AddStock(int productId, int quantity)
	{
		await Context.AddAsync(
			ProductStock.Create(
				GetContext<StorageTestContext>().Storages.First().Code,
				productId,
				quantity),
			CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
	}
}
