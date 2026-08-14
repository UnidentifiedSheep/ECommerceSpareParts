using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Models.CatalogueCandidate;
using Main.Entities.DomainEvents.CatalogueCandidate;
using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate.ContentChanged;

internal sealed class PublishIntegrationEvents(
    IIntegrationEventScope integrationEventScope,
    IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection,
    IReadRepository<Candidate, Guid> repository)
    : BatchableDomainEventHandler<CatalogueCandidateContentChangedDomainEvent>
{
    public override Task Handle(
        Batch<CatalogueCandidateContentChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        return CatalogueCandidateInternalService.PublishUpdatedEvents(
            integrationEventScope,
            projection,
            repository,
            notification.Items.Select(x => x.CatalogueCandidateId),
            cancellationToken);
    }
}
