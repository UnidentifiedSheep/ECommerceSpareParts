using Enums;
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
		Context.ChangeTracker.Clear();

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([TestContext.Candidate.Id]));

		result.Items.Should().ContainSingle();
		var item = result.Items[TestContext.Candidate.Id];

		var mapped = item.MappedCrosses.Should().ContainSingle().Subject;
		mapped.Id.Should().Be(TestContext.MappedCrossCandidate.Id);
		mapped.Sku.Should().Be("mapped-cross-candidate");
		mapped.Producer.Id.Should().Be(TestContext.Producer.Id);
		var mappedSupplierProduct = mapped.SupplierProducts.Should().ContainSingle().Subject;
		mappedSupplierProduct.Id.Should().Be(TestContext.MappedCross.Id);
		mappedSupplierProduct.Names.Should().ContainSingle(x => x.Name == "Mapped cross name");

		var notMapped = item.NotMappedCrosses.Should().ContainSingle().Subject;
		notMapped.Id.Should().Be(TestContext.NotMappedCross.Id);
		notMapped.Sku.Should().Be("not-mapped-cross");
		notMapped.Producer.Should().Be("Not mapped cross producer");
		notMapped.Supplier.Should().Be(Supplier.FavoritParts);
		notMapped.CandidateId.Should().BeNull();
		notMapped.Names.Should().ContainSingle(x => x.Name == "Not mapped cross name");
	}

	[Fact]
	public async Task SameCrossConnectedToMultipleSourceProducts_ReturnsCrossOnce()
	{
		await new SupplierProductCrossBuilder(Faker)
			.WithSupplierProducts(TestContext.ReverseSource, TestContext.MappedCross)
			.BuildAndAddToDb(Context);

		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([TestContext.Candidate.Id]));

		var item = result.Items[TestContext.Candidate.Id];
		item.MappedCrosses.Should().ContainSingle(x => x.Id == TestContext.MappedCrossCandidate.Id);
		item.NotMappedCrosses.Should().ContainSingle(x => x.Id == TestContext.NotMappedCross.Id);
	}

	[Fact]
	public async Task MultipleCandidatesRequested_ReturnsCrossesGroupedByCandidate()
	{
		var withCrosses = SupplierProductIdsWithCrosses();
		var withCross = SupplierTestContext
			.SupplierProducts
			.Where(x => withCrosses.Contains(x.Id) && x.CatalogueCandidateId != null)
			.Select(x => x.CatalogueCandidateId!.Value)
			.Take(2)
			.ToList();

		var act = () => Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([withCross[0], withCross[1]]),
			CancellationToken);

		var items = (await act.Should().NotThrowAsync()).Subject.Items;
		items.Keys.Should().BeEquivalentTo([withCross[0], withCross[1]]);

		var fSupplierProducts = CatalogueTestContext
			.Candidates
			.First(x => x.Id == withCross[0])
			.SupplierProducts
			.Select(x => x.Id)
			.ToHashSet();

		var fCrosses = SupplierTestContext.Crosses
			.Where(x =>
				fSupplierProducts.Contains(x.LeftId) ||
				fSupplierProducts.Contains(x.RightId))
			.SelectMany(x => new List<int> { x.LeftId, x.RightId })
			.Distinct()
			.Order()
			.ToList();

		items[withCross[0]].Should()

		result.Items[TestContext.Candidate.Id]
			.NotMappedCrosses
			.Should()
			.ContainSingle(x => x.Id == TestContext.NotMappedCross.Id);
		result.Items[TestContext.BatchCandidate.Id]
			.NotMappedCrosses
			.Should()
			.ContainSingle(x => x.Id == TestContext.BatchNotMappedCross.Id);
	}

	[Fact]
	public async Task CandidateWithoutCrosses_ReturnsNoItem()
	{
		var inCrossIds = SupplierProductIdsWithCrosses();

		var withOutCross = SupplierTestContext
			.SupplierProducts
			.First(x => !inCrossIds.Contains(x.Id) && x.CatalogueCandidateId != null)
			.CatalogueCandidateId!
			.Value;

		var act = () => Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([withOutCross]),
			CancellationToken);

		(await act.Should().NotThrowAsync()).Subject.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task UnknownCandidate_ReturnsNoItems()
	{
		var act = () => Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([Guid.NewGuid()]),
			CancellationToken);

		(await act.Should().NotThrowAsync()).Subject.Items.Should().BeEmpty();
	}

	private HashSet<int> SupplierProductIdsWithCrosses()
		=> SupplierTestContext
			.Crosses
			.SelectMany(x => new List<int>
			{
				x.LeftId, x.RightId
			})
			.ToHashSet();
}
