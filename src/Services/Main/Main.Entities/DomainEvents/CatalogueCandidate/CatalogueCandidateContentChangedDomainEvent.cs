using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.CatalogueCandidate;

public sealed record CatalogueCandidateContentChangedDomainEvent(
    Guid CatalogueCandidateId)
    : IBatchableDomainEvent, IKeyedDomainEvent
{
    public string GetKey()
        => $"catalogue:candidate:{CatalogueCandidateId}:content:changed";
}
