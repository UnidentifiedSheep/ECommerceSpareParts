using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment;
using Main.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;
using Main.Entities.Exceptions;
using Main.Enums.Products;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class CreateCandidateCrossesTests : IntegrationTest
{
	private CatalogueCandidateTestContext TestContext => GetContext<CatalogueCandidateTestContext>();
	public CreateCandidateCrossesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CatalogueCandidateTestContext>();
	}

	[Fact]
	public async Task WhenSelfReference_Throws()
	{
		var candidate = TestContext.Candidates.First();

		var act = () => Mediator.Send(
			new CreateCandidateCrossesCommand(
				candidate.Id,
				[candidate.Id],
				ProductLinkageType.FullCross),
			CancellationToken);

		await act.Should().ThrowAsync<ProductCrossSelfReferenceException>();
	}

	[Fact]
	public async Task WhenDataValid_Succeeds()
	{
		var leftId = TestContext.Candidates[0].Id;
		var otherIds = TestContext.Candidates.Skip(1).Select(x => x.Id).ToList();

		var act = () => Mediator.Send(
			new CreateCandidateCrossesCommand(
				leftId,
				otherIds,
				ProductLinkageType.SingleCross),
			CancellationToken);

		await act.Should().NotThrowAsync();

		var candidates = await Context.CatalogueCandidates
			.Include(x => x.Product)
			.AsNoTracking()
			.ToListAsync(CancellationToken);

		var crosses = (await Context.ProductCrosses
			.AsNoTracking()
			.ToListAsync(CancellationToken))
			.Select(x => new Tuple<Guid, Guid>(
				candidates.First(z => z.Product!.Id == x.RightProductId).Id,
				candidates.First(z => z.Product!.Id == x.LeftProductId).Id
				))
			.OrderBy(x => x.Item2)
			.ToList();

		var mustBe = otherIds
			.Select(x => new Tuple<Guid, Guid>(leftId, x))
			.OrderBy(x => x.Item2)
			.ToList();
		crosses.Should().BeEquivalentTo(mustBe);
	}

	[Fact]
	public async Task WhenEmptyList_Throws()
	{
		var act = () => Mediator.Send(
			new CreateCandidateCrossesCommand(
				TestContext.Candidates[0].Id,
				[],
				ProductLinkageType.SingleCross),
			CancellationToken);

		await act.Should().ThrowAsync<ValidationException>();
	}
}
