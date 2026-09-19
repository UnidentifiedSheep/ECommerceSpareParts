using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class GetCatalogueCandidateCrossesHandlerTests : IntegrationTest
{
	public GetCatalogueCandidateCrossesHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CatalogueCandidateTestContext>();
	}

	private CatalogueCandidateTestContext CatalogueTestContext => GetContext<CatalogueCandidateTestContext>();
	private SupplierProductTestContext SupplierTestContext => GetContext<SupplierProductTestContext>();

	[Fact]
	public async Task CandidateHasDirectAndReverseCrosses_ReturnsMappedAndNotMappedCrosses()
	{
		var candidate = CatalogueTestContext.Candidates[1];
		var expectedMappedIds = new[]
		{
			CatalogueTestContext.Candidates[0].Id, CatalogueTestContext.Candidates[2].Id
		};
		var expectedNotMappedId = SupplierTestContext.SupplierProducts[3].Id;
		Context.ChangeTracker.Clear();

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
		var mappedSupplierProduct = SupplierTestContext.SupplierProducts[1];
		var additionalSource = await new SupplierProductBuilder(Faker)
			.WithNamesCount(2)
			.BuildAndAddToDb(Context);
		candidate.AddSupplierProduct(additionalSource);
		await Context.SaveChangesAsync(CancellationToken);
		await new SupplierProductCrossBuilder(Faker)
			.WithSupplierProducts(additionalSource, mappedSupplierProduct)
			.BuildAndAddToDb(Context);

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
		var candidate = await new CatalogueCandidateBuilder(Faker)
			.WithProducerId(CatalogueTestContext.Candidates[0].ProducerId)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([candidate.Id]),
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
}
