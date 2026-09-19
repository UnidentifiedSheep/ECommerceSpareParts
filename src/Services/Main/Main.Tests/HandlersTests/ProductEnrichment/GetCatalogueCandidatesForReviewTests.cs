using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class GetCatalogueCandidatesForReviewTests : IntegrationTest
{
	public GetCatalogueCandidatesForReviewTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CatalogueCandidateTestContext>();
	}

	private CatalogueCandidateTestContext TestContext => GetContext<CatalogueCandidateTestContext>();

	[Fact]
	public async Task ExistingCandidate_ReturnsCompleteReviewProjection()
	{
		var candidate = TestContext.Candidates[0];
		var supplierProduct = candidate.SupplierProducts.Should().ContainSingle().Subject;
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(CreateQuery(sku: candidate.Sku.Value));

		var projectedCandidate = result.Candidates.Should().ContainSingle().Subject;
		projectedCandidate.Id.Should().Be(candidate.Id);
		projectedCandidate.Sku.Should().Be(candidate.Sku.Value);
		projectedCandidate.Producer.Id.Should().Be(candidate.ProducerId);
		projectedCandidate.Product.Should().BeNull();

		var projectedSupplierProduct = projectedCandidate.SupplierProducts.Should().ContainSingle().Subject;
		projectedSupplierProduct.Id.Should().Be(supplierProduct.Id);
		projectedSupplierProduct.Sku.Should().Be(supplierProduct.Sku.Value);
		projectedSupplierProduct.Producer.Should().Be(supplierProduct.Producer);
		projectedSupplierProduct.Supplier.Should().Be(supplierProduct.Supplier);
		projectedSupplierProduct.CandidateId.Should().Be(candidate.Id);
		projectedSupplierProduct.Names
			.Select(x => x.Name)
			.Should()
			.Equal(supplierProduct.Names.Select(x => x.Name));
	}

	[Fact]
	public async Task ProductIdSpecified_ReturnsOnlyMappedCandidate()
	{
		var producerId = TestContext.Candidates[0].ProducerId;
		var requestedProduct = await new ProductBuilder(Faker)
			.WithProducerId(producerId)
			.BuildAndAddToDb(Context);
		var requestedCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithProducerId(producerId)
			.WithProductId(requestedProduct.Id)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery(requestedProduct.Id));

		result.Candidates.Should().ContainSingle(x => x.Id == requestedCandidate.Id);
	}

	[Fact]
	public async Task SkuSpecified_SearchesByNormalizedExactValue()
	{
		var producerId = TestContext.Candidates[0].ProducerId;
		var requestedCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("AB-12 34")
			.WithProducerId(producerId)
			.BuildAndAddToDb(Context);
		await new CatalogueCandidateBuilder(Faker)
			.WithSku("AB-12 345")
			.WithProducerId(producerId)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery(sku: "ab_1234"));

		result.Candidates.Should().ContainSingle(x => x.Id == requestedCandidate.Id);
	}

	[Fact]
	public async Task ProductIdAndSkuSpecified_AppliesBothFilters()
	{
		var producerId = TestContext.Candidates[0].ProducerId;
		var product = await new ProductBuilder(Faker)
			.WithProducerId(producerId)
			.BuildAndAddToDb(Context);
		await new CatalogueCandidateBuilder(Faker)
			.WithSku("mapped-sku")
			.WithProducerId(producerId)
			.WithProductId(product.Id)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery(product.Id, "missing-sku"));

		result.Candidates.Should().BeEmpty();
	}

	private static GetCatalogueCandidatesForReviewQuery CreateQuery(int? productId = null, string? sku = null)
	{
		return new GetCatalogueCandidatesForReviewQuery(
			productId,
			sku,
			new Pagination(0, 20));
	}
}
