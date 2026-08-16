using Application.Common.Interfaces.Cqrs;
using Contracts.ProductEnrichment;
using Extensions;
using MediatR;
using Search.Application.Interfaces.CatalogueCandidate;
using CatalogueCandidateDocument = Search.Entities.CatalogueCandidate;

namespace Search.Application.Handlers.CatalogueCandidates.UpsertCatalogueCandidates;

public sealed record UpsertCatalogueCandidatesCommand(
    IReadOnlyCollection<CatalogueCandidateUpdatedEvent> Events)
    : ICommand;

public sealed class UpsertCatalogueCandidatesHandler(
    ICatalogueCandidateRepository repository)
    : ICommandHandler<UpsertCatalogueCandidatesCommand>
{
    public async Task<Unit> Handle(
        UpsertCatalogueCandidatesCommand request,
        CancellationToken cancellationToken)
    {
        var latestEvents = request.Events
            .GroupBy(x => x.Candidate.Id)
            .Select(group => group
                .OrderByDescending(x => x.OccuredAt)
                .First())
            .ToList();

        var mappedCandidateIds = latestEvents
            .Where(x => x.Candidate.MappedProductId.HasValue)
            .Select(x => x.Candidate.Id)
            .ToList();
        var documents = latestEvents
            .Where(x => !x.Candidate.MappedProductId.HasValue)
            .Select(MapDocument)
            .ToList();

        await repository.DeleteMany(
            mappedCandidateIds,
            cancellationToken);
        await repository.UpsertMany(
            documents,
            cancellationToken);

        return Unit.Value;
    }

    private static CatalogueCandidateDocument MapDocument(
        CatalogueCandidateUpdatedEvent @event)
    {
        var candidate = @event.Candidate;

        return new CatalogueCandidateDocument
        {
            Id = candidate.Id,
            Sku = candidate.Sku.Trim(),
            NormalizedSku = candidate.Sku.OnlyCharacterToLower(),
            ProducerId = candidate.ProducerId,
            MappedProductId = candidate.MappedProductId,
            Names = candidate.Names
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }
}
