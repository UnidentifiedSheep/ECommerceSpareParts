using Abstractions.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment.GetCatalogueCandidatesForReview;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class GetCatalogueCandidatesForReviewTests : IntegrationTest
{
	public GetCatalogueCandidatesForReviewTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CatalogueCandidateReviewTestContext>();
	}

	private CatalogueCandidateReviewTestContext TestContext =>
		GetContext<CatalogueCandidateReviewTestContext>();

	[Fact]
	public async Task ExistingCandidate_ReturnsCompleteReviewProjection()
	{
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(CreateQuery());

		var projectedCandidate = result.Candidates.Should().ContainSingle().Subject;
		projectedCandidate.Id.Should().Be(TestContext.Candidate.Id);
		projectedCandidate.Sku.Should().Be("review-sku");
		projectedCandidate.Producer.Id.Should().Be(TestContext.Producer.Id);
		projectedCandidate.Product!.Id.Should().Be(TestContext.Product.Id);

		var projectedSupplierProduct = projectedCandidate.SupplierProducts.Should().ContainSingle().Subject;
		projectedSupplierProduct.Id.Should().Be(TestContext.SupplierProduct.Id);
		projectedSupplierProduct.Sku.Should().Be("supplier-sku");
		projectedSupplierProduct.Producer.Should().Be("Supplier producer");
		projectedSupplierProduct.Supplier.Should().Be(Supplier.FavoritParts);
		projectedSupplierProduct
			.Names
			.Select(x => x.Name)
			.Should()
			.Equal("First supplier name", "Second supplier name");
	}

	[Fact]
	public async Task ProductIdSpecified_ReturnsOnlyMappedCandidate()
	{
		var skippedProduct = await new ProductBuilder(Faker)
			.WithProducerId(TestContext.Producer.Id)
			.BuildAndAddToDb(Context);
		await new CatalogueCandidateBuilder(Faker)
			.WithSku("skipped-sku")
			.WithProducerId(TestContext.Producer.Id)
			.WithProductId(skippedProduct.Id)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery(TestContext.Product.Id));

		result.Candidates.Should().ContainSingle(x => x.Id == TestContext.Candidate.Id);
	}

	[Fact]
	public async Task SkuSpecified_SearchesByNormalizedExactValue()
	{
		var requestedCandidate = await new CatalogueCandidateBuilder(Faker)
			.WithSku("AB-12 34")
			.WithProducerId(TestContext.Producer.Id)
			.BuildAndAddToDb(Context);
		await new CatalogueCandidateBuilder(Faker)
			.WithSku("AB-12 345")
			.WithProducerId(TestContext.Producer.Id)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(CreateQuery(sku: "ab_1234"));

		result.Candidates.Should().ContainSingle(x => x.Id == requestedCandidate.Id);
	}

	[Fact]
	public async Task ProductIdAndSkuSpecified_AppliesBothFilters()
	{
		var result = await Mediator.Send(CreateQuery(TestContext.Product.Id, "missing-sku"));

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
