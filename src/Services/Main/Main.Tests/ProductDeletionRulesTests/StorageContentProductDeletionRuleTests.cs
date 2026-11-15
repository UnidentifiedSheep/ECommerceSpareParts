using Main.Entities.Storage;
using Main.Application.Services.ProductDeletion;
using Main.Entities.Product;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.ProductDeletionRulesTests;

public class StorageContentProductDeletionRuleTests : ProductDeletionRuleTestsBase<StorageContentProductDeletionRule>
{
	public StorageContentProductDeletionRuleTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<StorageContentTestContext>();

	[Fact]
	public async Task ProductWithStorageContent_IsBlocked()
		=> await ShouldBlock(
			GetContext<StorageContentTestContext>().StorageContents.First().ProductId,
			StorageContentProductDeletionBlockedMessage.Key);

	[Fact]
	public async Task ProductWithoutStorageContent_IsNotBlocked()
		=> await ShouldNotBlock((await CreateUnreferencedProduct()).Id);
}
