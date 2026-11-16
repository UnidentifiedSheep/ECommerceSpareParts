using Application.Common.Abstractions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Contracts.Models.CatalogueCandidate;
using Main.Entities.Product.DomainEvents.CatalogueCandidate;
using Candidate = Main.Entities.Product.Product.Enrichment.CatalogueCandidate;

namespace Main.Modules.Product.Application.DomainEventHandlers.CatalogueCandidate.CatalogueCandidateContentChanged;

internal sealed class PublishIntegrationEventsHandler(
	IIntegrationEventScope integrationEventScope,
	IProjectionProvider<Candidate, CatalogueCandidateContractDto> projection,
	IReadRepository<Candidate, Guid> repository)
	: BatchableDomainEventHandler<CatalogueCandidateContentChangedDomainEvent>
{
	public override Task Handle(
		Batch<CatalogueCandidateContentChangedDomainEvent> notification,
		CancellationToken cancellationToken)
		=> CatalogueCandidateInternalService.PublishUpdatedEvents(
			integrationEventScope,
			projection,
			repository,
			notification.Items.Select(x => x.CatalogueCandidateId),
			cancellationToken);
}
