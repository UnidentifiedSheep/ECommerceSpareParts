using Application.Common.Models;
using FluentAssertions;
using Main.Entities.Product.Product;
using Main.Entities.Product.Product.Enrichment;
using Main.Modules.Product.Application.Queries.ProductEnrichment;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class GetCatalogueCandidatesForReviewTests(
	ReadScenarioFixture<GetCatalogueCandidatesForReviewTests.CandidatesForReviewScenario> fixture)
	: ReadScenarioTest<GetCatalogueCandidatesForReviewTests.CandidatesForReviewScenario>(fixture)
{
	[Fact]
	public async Task ExistingCandidate_ReturnsCompleteReviewProjection()
	{
		var candidate = Scenario.Get<CatalogueCandidateTestContext>().Candidates[0];
		var supplierProduct = candidate.SupplierProducts.Should().ContainSingle().Subject;

		var result = await Mediator.Send(CreateQuery(sku: candidate.Sku.Value), CancellationToken);

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
		projectedSupplierProduct
			.Names
			.Select(x => x.Name)
			.Should()
			.Equal(supplierProduct.Names.Select(x => x.Name));
	}

	[Fact]
	public async Task ProductIdSpecified_ReturnsOnlyMappedCandidate()
	{
		var result = await Mediator.Send(CreateQuery(Scenario.RequestedProduct.Id), CancellationToken);

		result.Candidates.Should().ContainSingle(x => x.Id == Scenario.RequestedCandidate.Id);
	}

	[Fact]
	public async Task SkuSpecified_SearchesByNormalizedExactValue()
	{
		var result = await Mediator.Send(CreateQuery(sku: "ab_1234"), CancellationToken);

		result.Candidates.Should().ContainSingle(x => x.Id == Scenario.SkuCandidate.Id);
	}

	[Fact]
	public async Task ProductIdAndSkuSpecified_AppliesBothFilters()
	{
		var result = await Mediator.Send(CreateQuery(Scenario.RequestedProduct.Id, "missing-sku"), CancellationToken);

		result.Candidates.Should().BeEmpty();
	}

	private static GetCatalogueCandidatesForReviewQuery CreateQuery(int? productId = null, string? sku = null)
		=> new(
			productId,
			sku,
			new Pagination(0, 20));

	public sealed class CandidatesForReviewScenario : ReadScenario
	{
		public CandidatesForReviewScenario() => RegisterBasicContext<CatalogueCandidateTestContext>();

		public Product RequestedProduct { get; private set; } = null!;

		public CatalogueCandidate RequestedCandidate { get; private set; } = null!;

		public CatalogueCandidate SkuCandidate { get; private set; } = null!;

		private int ProducerId => Get<CatalogueCandidateTestContext>().Candidates[0].ProducerId;

		protected override async Task PrepareAsync()
			=> RequestedProduct = await new ProductBuilder(Faker)
				.WithProducerId(ProducerId)
				.BuildAndAddToDb(Context, CancellationToken);

		protected override IEnumerable<object> Seed()
		{
			RequestedCandidate = new CatalogueCandidateBuilder(Faker)
				.WithSku("mapped-sku")
				.WithProducerId(ProducerId)
				.WithProductId(RequestedProduct.Id)
				.Build();
			SkuCandidate = new CatalogueCandidateBuilder(Faker)
				.WithSku("AB-12 34")
				.WithProducerId(ProducerId)
				.Build();
			var similarSkuCandidate = new CatalogueCandidateBuilder(Faker)
				.WithSku("AB-12 345")
				.WithProducerId(ProducerId)
				.Build();
			return [RequestedCandidate, SkuCandidate, similarSkuCandidate];
		}
	}
}
