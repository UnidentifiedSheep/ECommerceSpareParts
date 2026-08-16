using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Microsoft.EntityFrameworkCore;
using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate;

internal static class CatalogueCandidateInternalService
{
    public static async Task PublishUpdatedEvents(
        IIntegrationEventScope integrationEventScope,
        IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection,
        IReadRepository<Candidate, Guid> repository,
        IEnumerable<Guid> candidateIds,
        CancellationToken cancellationToken)
    {
        var occurredAt = DateTime.UtcNow;
        var chunkedIds = candidateIds
            .Distinct()
            .Chunk(1000);

        foreach (var ids in chunkedIds)
        {
            var candidates = await repository.Query
                .Where(x => ids.AsEnumerable().Contains(x.Id))
                .Project(projection)
                .ToListAsync(cancellationToken);

            var events = candidates
                .Select(candidate => new CatalogueCandidateUpdatedEvent
                {
                    OccuredAt = occurredAt,
                    Candidate = candidate with
                    {
                        Names = candidate.Names
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList()
                    }
                })
                .ToList();

            integrationEventScope.AddRange(events);
        }
    }
}
