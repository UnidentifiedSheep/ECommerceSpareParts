namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseCandidate(int ProductId, IReadOnlySet<Guid> SaleIds);
