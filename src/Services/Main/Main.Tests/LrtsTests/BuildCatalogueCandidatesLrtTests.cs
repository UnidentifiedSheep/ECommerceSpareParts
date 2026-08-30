using Domain.CommonEnums;
using Enums;
using FluentAssertions;
using Main.Application.Lrts.BuildCatalogueCandidates;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class BuildCatalogueCandidatesLrtTests : LrtIntegrationTest<BuildCatalogueCandidatesLrt>
{
	public BuildCatalogueCandidatesLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Fact]
	public async Task ProductsWithSameResolvedKey_CreateOneCandidateAndAssignAllProducts()
	{
		var producer = TestContext.Producers[0];
		var firstProduct = await AddSupplierProduct(
			"ABC-123",
			producer.Name,
			Supplier.Armtek);
		var secondProduct = await AddSupplierProduct(
			"ABC123",
			producer.Name,
			Supplier.Tmtr);

		var execution = await ExecuteLrt();
		var state = execution.GetState<BuildCatalogueCandidatesState>();

		execution.Job.Status.Should().Be(JobStatus.Succeeded);
		state.LastProcessedId.Should().Be(Math.Max(firstProduct.Id, secondProduct.Id));
		state.ProcessedRows.Should().Be(2);
		state.AssignedRows.Should().Be(2);
		state.SkippedRows.Should().Be(0);

		var candidate = await Context.CatalogueCandidates.AsNoTracking().SingleAsync();
		candidate.ProducerId.Should().Be(producer.Id);
		candidate.Sku.NormalizedValue.Should().Be("ABC123");

		var candidateIds = await Context
			.SupplierProducts
			.AsNoTracking()
			.Select(x => x.CatalogueCandidateId)
			.ToListAsync();
		candidateIds.Should().OnlyContain(x => x == candidate.Id);
	}

	[Fact]
	public async Task ExistingCandidate_IsReused()
	{
		var producer = TestContext.Producers[0];
		var candidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("ABC-123")
			.WithProducerId(producer.Id)
			.BuildAndAddToDb(Context);
		var supplierProduct = await AddSupplierProduct(
			"ABC123",
			producer.Name,
			Supplier.Armtek);

		var execution = await ExecuteLrt();
		var state = execution.GetState<BuildCatalogueCandidatesState>();

		execution.Job.Status.Should().Be(JobStatus.Succeeded);
		state.AssignedRows.Should().Be(1);
		(await Context.CatalogueCandidates.CountAsync()).Should().Be(1);

		Context.ChangeTracker.Clear();
		var persistedProduct = await Context
			.SupplierProducts
			.AsNoTracking()
			.SingleAsync(x => x.Id == supplierProduct.Id);
		persistedProduct.CatalogueCandidateId.Should().Be(candidate.Id);
	}

	[Fact]
	public async Task UnknownProducer_IsSkippedAndCursorStillAdvances()
	{
		var supplierProduct = await AddSupplierProduct(
			"UNKNOWN-123",
			$"unknown-{Guid.NewGuid():N}",
			Supplier.Armtek);

		var execution = await ExecuteLrt();
		var state = execution.GetState<BuildCatalogueCandidatesState>();

		execution.Job.Status.Should().Be(JobStatus.Succeeded);
		state.LastProcessedId.Should().Be(supplierProduct.Id);
		state.ProcessedRows.Should().Be(1);
		state.AssignedRows.Should().Be(0);
		state.SkippedRows.Should().Be(1);
		(await Context.CatalogueCandidates.CountAsync()).Should().Be(0);

		Context.ChangeTracker.Clear();
		var persistedProduct = await Context
			.SupplierProducts
			.AsNoTracking()
			.SingleAsync(x => x.Id == supplierProduct.Id);
		persistedProduct.CatalogueCandidateId.Should().BeNull();
	}

	private Task<SupplierProduct> AddSupplierProduct(
		string sku,
		string producer,
		Supplier supplier)
	{
		return new SupplierProductBuilder(Faker)
			.WithSku(sku)
			.WithProducer(producer)
			.WithSupplier(supplier)
			.BuildAndAddToDb(Context);
	}
}
