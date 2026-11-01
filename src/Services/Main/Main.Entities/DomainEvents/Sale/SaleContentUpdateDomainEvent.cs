using Domain.Interfaces.Events;
using Main.Entities.Sale;

namespace Main.Entities.DomainEvents.Sale;

public sealed record SaleContentUpdateDomainEvent(SaleContent Content, bool FinancialInfoChanged) : IBatchableDomainEvent;
