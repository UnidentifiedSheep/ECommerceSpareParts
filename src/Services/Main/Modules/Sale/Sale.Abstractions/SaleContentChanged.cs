using Domain.Interfaces.Events;

namespace Main.Modules.Sale.Abstractions;

public sealed record SaleContentChanged(SaleContentSnapshot Snapshot) : IBatchableDomainEvent;
