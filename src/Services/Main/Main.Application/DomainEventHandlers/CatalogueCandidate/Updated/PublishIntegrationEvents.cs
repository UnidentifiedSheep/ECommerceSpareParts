using Application.Common.Abstractions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Domain.Events;
using Microsoft.EntityFrameworkCore;
using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate.Updated;

public class PublishIntegrationEvents(
    IIntegrationEventScope integrationEventScope,
    IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection,
    IReadRepository<Candidate, int> repository
    ) : BatchableDomainEventHandler<EntityUpdatedDomainEvent<Candidate, int>>
{
    public override async Task Handle(
        Batch<EntityUpdatedDomainEvent<Candidate, int>> notification, 
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        
        var chunkedIds = notification.Items
            .Select(x => x.Id)
            .Distinct()
            .Chunk(1000)
            .Select(x => x.ToList());

        foreach (var ids in chunkedIds)
        {
            var events = (await repository.Query
                .Where(x => ids.Contains(x.Id))
                .Project(projection)
                .ToListAsync(cancellationToken))
                .Select(x => new CatalogueCandidateUpdatedEvent
                {
                    OccuredAt = now,
                    Candidate = x
                })
                .ToList();
            
            integrationEventScope.AddRange(events);
        }
    }
}