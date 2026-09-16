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
		RegisterBasicContext<CatalogueCandidateCrossesTestContext>();
	}

	private CatalogueCandidateCrossesTestContext TestContext =>
		GetContext<CatalogueCandidateCrossesTestContext>();

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
		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery(
				[TestContext.Candidate.Id, TestContext.BatchCandidate.Id]));

		result.Items.Keys.Should().BeEquivalentTo(
			[TestContext.Candidate.Id, TestContext.BatchCandidate.Id]);
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
		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([TestContext.CandidateWithoutCrosses.Id]));

		result.Items.Should().BeEmpty();
	}

	[Fact]
	public async Task UnknownCandidate_ReturnsNoItems()
	{
		var result = await Mediator.Send(
			new GetCatalogueCandidateCrossesQuery([Guid.NewGuid()]));

		result.Items.Should().BeEmpty();
	}
}
