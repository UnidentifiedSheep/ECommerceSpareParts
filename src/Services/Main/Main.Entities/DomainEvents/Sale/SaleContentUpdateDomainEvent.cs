using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.Sale;

public sealed record SaleContentUpdateDomainEvent(Guid SaleId, bool FinancialInfoChanged) : IBatchableDomainEvent;
