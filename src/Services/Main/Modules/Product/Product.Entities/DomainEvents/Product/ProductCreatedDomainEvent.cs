using Domain.Interfaces.Events;

namespace Main.Entities.Product.DomainEvents.Product;

public record ProductCreatedDomainEvent(Entities.Product.Product.Product Product) : IBatchableDomainEvent;
