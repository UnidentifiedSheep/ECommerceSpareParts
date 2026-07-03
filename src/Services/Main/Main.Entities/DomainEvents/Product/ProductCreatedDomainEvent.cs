using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Product;

public record ProductCreatedDomainEvent(Entities.Product.Product Product) : IBatchableDomainEvent;