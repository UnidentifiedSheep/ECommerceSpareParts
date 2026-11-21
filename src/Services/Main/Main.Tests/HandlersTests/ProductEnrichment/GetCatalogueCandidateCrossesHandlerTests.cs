using FluentAssertions;
using Main.Entities.Product.Product.Enrichment;
using Main.Modules.Product.Application.Queries.ProductEnrichment;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class GetCatalogueCandidateCrossesHandlerTests(
	ReadScenarioFixture<GetCatalogueCandidateCrossesHandlerTests.CandidateCrossesScenario> fixture)
	: ReadScenarioTest<GetCatalogueCandidateCrossesHandlerTests.CandidateCrossesScenario>(fixture)
{
	private CatalogueCandidateTestContext CatalogueTestContext => Scenario.Get<CatalogueCandidateTestContext>();
	private SupplierProductTestContext SupplierTestContext => Scenario.Get<SupplierProductTestContext>();

	[Fact]
	public async Task CandidateHasDirectAndReverseCrosses_ReturnsMappedAndNotMappedCrosses()
	{
		var candidate = CatalogueTestContext.Candidates[1];
		var expectedMappedIds = new[]
		{
			CatalogueTestContext.Candidates[0].Id, CatalogueTestContext.Candidates[2].Id
		};
		var expectedNotMappedId = SupplierTestContext.SupplierProducts[3].Id;

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([candidate.Id]),
			CancellationToken);

		var item = result.Items.Should().ContainSingle().Subject.Value;
		item.MappedCrosses.Select(x => x.Id).Should().BeEquivalentTo(expectedMappedIds);
		item.NotMappedCrosses.Select(x => x.Id).Should().BeEquivalentTo([expectedNotMappedId]);
	}

	[Fact]
	public async Task SameCrossConnectedToMultipleSourceProducts_ReturnsCrossOnce()
	{
		var candidate = CatalogueTestContext.Candidates[0];
		var mappedCandidate = CatalogueTestContext.Candidates[1];

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([candidate.Id]),
			CancellationToken);

		result.Items[candidate.Id].MappedCrosses.Should().ContainSingle(x => x.Id == mappedCandidate.Id);
	}

	[Fact]
	public async Task MultipleCandidatesRequested_ReturnsCrossesGroupedByCandidate()
	{
		var firstCandidate = CatalogueTestContext.Candidates[0];
		var secondCandidate = CatalogueTestContext.Candidates[1];
		var thirdCandidate = CatalogueTestContext.Candidates[2];
		var notMappedProduct = SupplierTestContext.SupplierProducts[3];

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([firstCandidate.Id, secondCandidate.Id]),
			CancellationToken);

		result.Items.Keys.Should().BeEquivalentTo([firstCandidate.Id, secondCandidate.Id]);
		result
			.Items[firstCandidate.Id]
			.MappedCrosses
			.Select(x => x.Id)
			.Should()
			.BeEquivalentTo([secondCandidate.Id, thirdCandidate.Id]);
		result.Items[firstCandidate.Id].NotMappedCrosses.Should().BeEmpty();
		result
			.Items[secondCandidate.Id]
			.MappedCrosses
			.Select(x => x.Id)
			.Should()
			.BeEquivalentTo([firstCandidate.Id, thirdCandidate.Id]);
		result
			.Items[secondCandidate.Id]
			.NotMappedCrosses
			.Select(x => x.Id)
			.Should()
			.BeEquivalentTo([notMappedProduct.Id]);
	}

	[Fact]
	public async Task CandidateWithoutCrosses_ReturnsNoItem()
	{
		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([Scenario.CandidateWithoutCrosses.Id]),
			CancellationToken);

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task UnknownCandidate_ReturnsNoItems()
	{
		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([Guid.NewGuid()]),
			CancellationToken);

		result.Items.Should().BeEmpty();
	}

	public sealed class CandidateCrossesScenario : ReadScenario
	{
		private SupplierProduct _additionalSource = null!;

		public CandidateCrossesScenario() => RegisterBasicContext<CatalogueCandidateTestContext>();

		public CatalogueCandidate CandidateWithoutCrosses { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			_additionalSource = await new SupplierProductBuilder(Faker)
				.WithNamesCount(2)
				.BuildAndAddToDb(Context, CancellationToken);
			Get<CatalogueCandidateTestContext>().Candidates[0].AddSupplierProduct(_additionalSource);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			var candidates = Get<CatalogueCandidateTestContext>().Candidates;
			var cross = new SupplierProductCrossBuilder(Faker)
				.WithSupplierProducts(_additionalSource, Get<SupplierProductTestContext>().SupplierProducts[1])
				.Build();
			CandidateWithoutCrosses = new CatalogueCandidateBuilder(Faker)
				.WithProducerId(candidates[0].ProducerId)
				.Build();
			return [cross, CandidateWithoutCrosses];
		}
	}
}
