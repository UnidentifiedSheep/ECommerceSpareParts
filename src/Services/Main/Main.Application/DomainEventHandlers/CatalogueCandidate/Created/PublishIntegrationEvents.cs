using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Services.Events;
using Contracts.Models.CatalogueCandidate;
using Contracts.ProductEnrichment;
using Domain.Events;

using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate.Created;

public class PublishIntegrationEvents(
    IIntegrationEventScope integrationEventScope,
    IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection
    ) : BatchableDomainEventHandler<EntityCreatedDomainEvent<Candidate>>
{
    public override Task Handle(
        Batch<EntityCreatedDomainEvent<Candidate>> notification, 
        CancellationToken cancellationToken)
    {
        var events = new List<CatalogueCandidateUpdatedEvent>();
        var now = DateTime.UtcNow;
        
        foreach (var @event in notification.Items)
        {
            if (@event.Entity.Id == 0)
                throw new InvalidOperationException("Save Changes should be called before domain event handlers.");
            
            events.Add(new CatalogueCandidateUpdatedEvent
            {
                OccuredAt = now,
                Candidate = projection.ProjectionFunc(@event.Entity)
            });
        }
        
        integrationEventScope.AddRange(events);
        return Task.CompletedTask;
    }
}