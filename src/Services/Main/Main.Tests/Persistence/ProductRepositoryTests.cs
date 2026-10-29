using FluentAssertions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.Persistence;

public sealed class ProductRepositoryTests : IntegrationTest
{
	public ProductRepositoryTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProducerTestContext>();

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();
	private IProductRepository Repository => Scope.ServiceProvider.GetRequiredService<IProductRepository>();

	[Fact]
	public async Task InsertProducts_ReturnsGeneratedIdsOfInsertedProducts()
	{
		var products = new ProductBuilder(Faker)
			.WithProducers(TestContext.Producers)
			.BuildMany(3);

		var ids = await InsertProducts(products);

		ids.Should().HaveCount(3).And.OnlyHaveUniqueItems().And.OnlyContain(id => id > 0);
		var savedIds = await Context.Products.AsNoTracking().Select(product => product.Id)
			.ToListAsync(CancellationToken);
		ids.Should().BeEquivalentTo(savedIds);
	}

	[Fact]
	public async Task InsertProducts_ExistingAndBatchDuplicates_ReturnsOnlyNewIdsWithoutMixingProducers()
	{
		var firstProducer = TestContext.Producers[0];
		var secondProducer = TestContext.Producers[1];
		var existing = await new ProductBuilder(Faker)
			.WithProducerId(firstProducer.Id).WithSku(new Sku("ABC-123"))
			.WithName("Original product").BuildAndAddToDb(Context, CancellationToken);
		var unrelated = await new ProductBuilder(Faker)
			.WithProducerId(firstProducer.Id).WithSku(new Sku("OTHER-123"))
			.BuildAndAddToDb(Context, CancellationToken);

		var ids = await InsertProducts(
		[
			existing,
			new ProductBuilder(Faker).WithProducerId(firstProducer.Id)
				.WithSku(new Sku("ABC 123")).WithName("Existing duplicate").Build(),
			new ProductBuilder(Faker).WithProducerId(firstProducer.Id)
				.WithSku(new Sku("XYZ-123")).WithName("First new product").Build(),
			new ProductBuilder(Faker).WithProducerId(firstProducer.Id)
				.WithSku(new Sku("XYZ 123")).WithName("Batch duplicate").Build(),
			new ProductBuilder(Faker).WithProducerId(secondProducer.Id)
				.WithSku(new Sku("ABC123")).WithName("Another producer").Build()
		]);

		var saved = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);
		saved.Should().HaveCount(4);
		ids.Should().HaveCount(2).And.BeEquivalentTo(saved
			.Where(product => product.Id != existing.Id && product.Id != unrelated.Id)
			.Select(product => product.Id));
		saved.Single(product => product.Id == existing.Id).Name.Value.Should().Be("Original product");
		saved.Single(product => product.Sku.NormalizedValue == "XYZ123").Name.Value
			.Should().Be("First new product");
		saved.Should().ContainSingle(product => product.ProducerId == secondProducer.Id &&
			product.Sku.NormalizedValue == "ABC123");
	}

	[Fact]
	public async Task InsertProducts_RepeatedBatch_ReturnsNoIds()
	{
		var products = new ProductBuilder(Faker)
			.WithProducerId(TestContext.Producers[0].Id)
			.BuildMany(2);
		var originalIds = await InsertProducts(products);

		var repeatedIds = await InsertProducts(products);

		repeatedIds.Should().BeEmpty();
		var savedIds = await Context.Products.AsNoTracking().Select(product => product.Id)
			.ToListAsync(CancellationToken);
		savedIds.Should().BeEquivalentTo(originalIds);
	}

	[Fact]
	public async Task InsertProducts_EmptyBatch_ReturnsNoIds()
	{
		var ids = await Repository.InsertProductsOnConflictDoNothingAsync([], CancellationToken);

		ids.Should().BeEmpty();
		(await Context.Products.AnyAsync(CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public async Task InsertProducts_Rollback_DoesNotPersistProductsAndAllowsRetry()
	{
		var products = new ProductBuilder(Faker)
			.WithProducerId(TestContext.Producers[0].Id)
			.BuildMany(2);
		await using (var transaction = await Context.Database.BeginTransactionAsync(CancellationToken))
		{
			var rolledBackIds = await Repository.InsertProductsOnConflictDoNothingAsync(products, CancellationToken);
			rolledBackIds.Should().HaveCount(2).And.OnlyContain(id => id > 0);
			await transaction.RollbackAsync(CancellationToken);
		}
		(await Context.Products.AnyAsync(CancellationToken)).Should().BeFalse();

		var ids = await InsertProducts(products);

		ids.Should().HaveCount(2).And.OnlyContain(id => id > 0);
		var savedIds = await Context.Products.AsNoTracking().Select(product => product.Id)
			.ToListAsync(CancellationToken);
		ids.Should().BeEquivalentTo(savedIds);
	}

	private async Task<IReadOnlyList<int>> InsertProducts(IReadOnlyCollection<Product> products)
	{
		await using var transaction = await Context.Database.BeginTransactionAsync(CancellationToken);
		var ids = await Repository.InsertProductsOnConflictDoNothingAsync(products, CancellationToken);
		await transaction.CommitAsync(CancellationToken);
		return ids;
	}
}
