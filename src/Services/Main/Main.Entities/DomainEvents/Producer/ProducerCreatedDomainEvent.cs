using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Producer;

public record ProducerCreatedDomainEvent(
    Entities.Producer.Producer Producer) : IBatchableDomainEvent;