using Domain.Interfaces.Events;
using Main.Entities.Sale.Sale;

namespace Main.Entities.Sale.DomainEvents;

public sealed record SaleContentUpdateDomainEvent(SaleContent Content, bool FinancialInfoChanged)
	: IBatchableDomainEvent;
