using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using FluentAssertions;
using Moq;
using Search.Application.Handlers.CatalogueCandidates.UpsertCatalogueCandidates;
using Search.Application.Interfaces.CatalogueCandidate;
using CatalogueCandidateDocument = Search.Entities.CatalogueCandidate;

namespace Search.Tests.Handlers.CatalogueCandidates;

public class UpsertCatalogueCandidatesHandlerTests
{
	private readonly Mock<ICatalogueCandidateRepository> _repository = new();

	[Fact]
	public async Task Handle_WhenCandidateIsNotMapped_ShouldUpsertNormalizedDocument()
	{
		var candidateId = Guid.NewGuid();
		var @event = CreateEvent(
			candidateId,
			"  AbC-12  ",
			null,
			[" First name ", "first NAME", "", "Second name"]);
		List<CatalogueCandidateDocument> upsertedDocuments = [];
		List<Guid> deletedIds = [];
		SetupRepository(upsertedDocuments, deletedIds);
		var handler = new UpsertCatalogueCandidatesHandler(_repository.Object);

		await handler.Handle(new UpsertCatalogueCandidatesCommand([@event]), CancellationToken.None);

		upsertedDocuments
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeEquivalentTo(
				new CatalogueCandidateDocument
				{
					Id = candidateId,
					Sku = "AbC-12",
					NormalizedSku = "abc12",
					ProducerId = 42,
					MappedProductId = null,
					Names = ["First name", "Second name"]
				});
		deletedIds.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WhenCandidateIsMapped_ShouldDeleteCandidateDocument()
	{
		var candidateId = Guid.NewGuid();
		var @event = CreateEvent(candidateId, mappedProductId: 123);
		List<CatalogueCandidateDocument> upsertedDocuments = [];
		List<Guid> deletedIds = [];
		SetupRepository(upsertedDocuments, deletedIds);
		var handler = new UpsertCatalogueCandidatesHandler(_repository.Object);

		await handler.Handle(new UpsertCatalogueCandidatesCommand([@event]), CancellationToken.None);

		deletedIds.Should().Equal(candidateId);
		upsertedDocuments.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WhenSeveralEventsForCandidate_ShouldApplyNewestEventOnly()
	{
		var candidateId = Guid.NewGuid();
		var olderEvent = CreateEvent(
			candidateId,
			mappedProductId: null,
			occuredAt: new DateTime(
				2026,
				8,
				16,
				10,
				0,
				0,
				DateTimeKind.Utc));
		var newerEvent = CreateEvent(
			candidateId,
			mappedProductId: 123,
			occuredAt: new DateTime(
				2026,
				8,
				16,
				11,
				0,
				0,
				DateTimeKind.Utc));
		List<CatalogueCandidateDocument> upsertedDocuments = [];
		List<Guid> deletedIds = [];
		SetupRepository(upsertedDocuments, deletedIds);
		var handler = new UpsertCatalogueCandidatesHandler(_repository.Object);

		await handler.Handle(
			new UpsertCatalogueCandidatesCommand([olderEvent, newerEvent]),
			CancellationToken.None);

		deletedIds.Should().Equal(candidateId);
		upsertedDocuments.Should().BeEmpty();
	}

	private void SetupRepository(
		ICollection<CatalogueCandidateDocument> upsertedDocuments,
		ICollection<Guid> deletedIds)
	{
		_repository
			.Setup(x => x.UpsertMany(
				It.IsAny<IEnumerable<CatalogueCandidateDocument>>(),
				It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<CatalogueCandidateDocument>, CancellationToken>((documents, _) =>
			{
				foreach (var document in documents)
					upsertedDocuments.Add(document);
			})
			.Returns(Task.CompletedTask);
		_repository
			.Setup(x => x.DeleteMany(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
			.Callback<IEnumerable<Guid>, CancellationToken>((ids, _) =>
			{
				foreach (var id in ids)
					deletedIds.Add(id);
			})
			.Returns(Task.CompletedTask);
	}

	private static CatalogueCandidateUpdatedEvent CreateEvent(
		Guid id,
		string sku = "SKU-1",
		int? mappedProductId = null,
		IReadOnlyList<string>? names = null,
		DateTime? occuredAt = null)
	{
		return new CatalogueCandidateUpdatedEvent
		{
			Candidate = new CatalogueCandidateContractDto
			{
				Id = id,
				Sku = sku,
				ProducerId = 42,
				MappedProductId = mappedProductId,
				Names = names ?? ["Candidate name"]
			},
			OccuredAt = occuredAt ?? DateTime.UtcNow
		};
	}
}
