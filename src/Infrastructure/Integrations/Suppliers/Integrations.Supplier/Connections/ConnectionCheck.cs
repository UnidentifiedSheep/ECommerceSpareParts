using Integrations.Supplier.Enums;

namespace Integrations.Supplier.Connections;

public record ConnectionCheck<TConnection>(
    bool CanUse,
    TConnection? Connection,
    SupplierUnavailableReason? Reason = null,
    string? Message = null
);