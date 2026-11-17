using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Entities.Product;
using Main.Entities.Product.Enums.Products;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using Main.Entities.Product.Product.Enrichment;
using Main.Modules.Product.Application.Handlers.ProductEnrichment.CreateCandidateCrosses;
using Microsoft.EntityFrameworkCore;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;
using Tests.TestContexts.ProductEnrichment;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class CreateCandidateCrossesTests : IntegrationTest
{
	public CreateCandidateCrossesTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<CatalogueCandidateTestContext>();
	private CatalogueCandidateTestContext TestContext => GetContext<CatalogueCandidateTestContext>();

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

		var candidates = await Context
			.Set<CatalogueCandidate>()
			.Include(x => x.Product)
			.AsNoTracking()
			.ToListAsync(CancellationToken);

		var crosses = (await Context.Set<ProductCross>().AsNoTracking().ToListAsync(CancellationToken))
			.Select(x => new Tuple<Guid, Guid>(
				candidates.First(z => z.Product!.Id == x.RightProductId).Id,
				candidates.First(z => z.Product!.Id == x.LeftProductId).Id))
			.OrderBy(x => x.Item2)
			.ToList();

		var mustBe = otherIds.Select(x => new Tuple<Guid, Guid>(leftId, x)).OrderBy(x => x.Item2).ToList();
		crosses.Should().BeEquivalentTo(mustBe);
	}
}

public class
	CreateCandidateCrossesValidationTests : ValidationTest<CreateCandidateCrossesValidation,
	CreateCandidateCrossesCommand>
{
	[Fact]
	public async Task WhenEmptyList_Throws()
	{
		var result = await ValidateAsync(
			new CreateCandidateCrossesCommand(
				Guid.NewGuid(),
				[],
				ProductLinkageType.SingleCross));
		result.ShouldHaveValidationErrorFor(x => x.CrossCandidateIds)
			.WithErrorCode(CatalogueCandidateCrossesNotEmptyMessage.Instance.MessageKey)
			.WithCustomState(CatalogueCandidateCrossesNotEmptyMessage.Instance)
			.Only();
	}
}
