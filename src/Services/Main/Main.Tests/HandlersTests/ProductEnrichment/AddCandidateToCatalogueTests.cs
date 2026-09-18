using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment;
using Main.Entities.Exceptions;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class AddCandidateToCatalogueTests : IntegrationTest
{
	public AddCandidateToCatalogueTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Fact]
	public async Task Batch_WithSelectedAndDefaultNames_CreatesAllProducts()
	{
		var selectedNameCandidate = await CreateCandidate("BATCH-SELECTED", "Ignored name");
		var defaultNameCandidate = await CreateCandidate("BATCH-DEFAULT", "Default name");

		await Mediator.Send(new AddCandidateToCatalogueCommand(
			[
				new AddCandidateToCatalogueItem(selectedNameCandidate.Id, "Selected name"),
				new AddCandidateToCatalogueItem(defaultNameCandidate.Id, null)
			]), CancellationToken);

		Context.ChangeTracker.Clear();
		var products = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);

		products.Should().HaveCount(2);
		products
			.Single(x => x.Sku.Value == "BATCH-SELECTED")
			.Name.Value.Should().Be("Selected name");
		products
			.Single(x => x.Sku.Value == "BATCH-DEFAULT")
			.Name.Value.Should().Be("Default name");
	}

	[Fact]
	public async Task Batch_WithAlreadyMappedCandidate_SkipsItAndCreatesRemainingProduct()
	{
		var existingProduct = await new ProductBuilder(Faker)
			.WithProducerId(TestContext.Producers[0].Id)
			.BuildAndAddToDb(Context);
		var mappedCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("BATCH-MAPPED")
			.WithProducerId(TestContext.Producers[0].Id)
			.WithProductId(existingProduct.Id)
			.BuildAndAddToDb(Context);
		var newCandidate = await CreateCandidate("BATCH-NEW", "New product");

		await Mediator.Send(
			new AddCandidateToCatalogueCommand(
			[
				new AddCandidateToCatalogueItem(mappedCandidate.Id, "Must not be created"),
				new AddCandidateToCatalogueItem(newCandidate.Id, null)
			]),
			CancellationToken);

		Context.ChangeTracker.Clear();
		var products = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);

		products.Should().HaveCount(2);
		products.Should().ContainSingle(x => x.Id == existingProduct.Id);
		products.Should().ContainSingle(x => x.Sku.Value == "BATCH-NEW" && x.Name.Value == "New product");
	}

	[Fact]
	public async Task Batch_WithMissingCandidate_ThrowsAndCreatesNothing()
	{
		var candidate = await CreateCandidate("BATCH-EXISTING", "Existing candidate");
		var command = new AddCandidateToCatalogueCommand(
		[
			new AddCandidateToCatalogueItem(candidate.Id, null),
			new AddCandidateToCatalogueItem(Guid.CreateVersion7(), "Missing candidate")
		]);

		var action = () => Mediator.Send(command);

		await action.Should().ThrowAsync<CatalogueCandidateNotFoundException>();
		Context.ChangeTracker.Clear();
		(await Context.Products.AsNoTracking().CountAsync(CancellationToken)).Should().Be(0);
	}

	[Fact]
	public async Task Batch_WithDuplicateCandidateIds_ThrowsAndCreatesNothing()
	{
		var candidate = await CreateCandidate("BATCH-DUPLICATE", "Default name");
		var command = new AddCandidateToCatalogueCommand(
		[
			new AddCandidateToCatalogueItem(candidate.Id, "First name"),
			new AddCandidateToCatalogueItem(candidate.Id, "Second name")
		]);

		var action = () => Mediator.Send(command);

		await action.Should().ThrowAsync<CatalogueCandidateDuplicateIdsException>();
		Context.ChangeTracker.Clear();
		(await Context.Products.AsNoTracking().CountAsync(CancellationToken)).Should().Be(0);
	}

	private async Task<CatalogueCandidate> CreateCandidate(string sku, params string[] names)
	{
		var candidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku(sku)
			.WithProducerId(TestContext.Producers[0].Id)
			.BuildAndAddToDb(Context);
		var supplierProduct = new SupplierProductBuilder(Faker).Build();

		candidate.AddSupplierProduct(supplierProduct);
		foreach (var name in names)
			supplierProduct.AddName(name);

		await Context.AddAsync(supplierProduct);
		await Context.SaveChangesAsync();

		return candidate;
	}
}
