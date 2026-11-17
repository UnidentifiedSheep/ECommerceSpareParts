using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.Producer;

public record ProducerCreatedDomainEvent(Entities.Product.Producer.Producer Producer) : IBatchableDomainEvent;
