using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Services.Events;
using Contracts.ProductEnrichment;
using Domain.Events;
using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate.Deleted;

public class PublishIntegrationEvents(
    IIntegrationEventScope integrationEventScope
    ) : BatchableDomainEventHandler<EntityDeletedDomainEvent<Candidate, int>>
{
    public override Task Handle(
        Batch<EntityDeletedDomainEvent<Candidate, int>> notification, 
        CancellationToken cancellationToken)
    {
        var events = notification
            .Items
            .Select(x => x.Id)
            .Select(x => new CatalogueCandidateDeletedEvent
            {
                Id = x
            })
            .ToList();
        
        integrationEventScope.AddRange(events);
        return Task.CompletedTask;
    }
}