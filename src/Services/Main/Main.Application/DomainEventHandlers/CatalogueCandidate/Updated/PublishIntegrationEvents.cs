using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Models.CatalogueCandidate;
using Domain.Events;
using Candidate = Main.Entities.Product.Enrichment.CatalogueCandidate;

namespace Main.Application.DomainEventHandlers.CatalogueCandidate.Updated;

internal sealed class PublishIntegrationEvents(
    IIntegrationEventScope integrationEventScope,
    IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection,
    IReadRepository<Candidate, Guid> repository)
    : BatchableDomainEventHandler<EntityUpdatedDomainEvent<Candidate, Guid>>
{
    public override Task Handle(
        Batch<EntityUpdatedDomainEvent<Candidate, Guid>> notification,
        CancellationToken cancellationToken)
    {
        return CatalogueCandidateInternalService.PublishUpdatedEvents(
            integrationEventScope,
            projection,
            repository,
            notification.Items.Select(x => x.Id),
            cancellationToken);
    }
}
