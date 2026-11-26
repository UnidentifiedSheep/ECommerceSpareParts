namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseProductResult(
	int ProductId,
	int SalesCount,
	IReadOnlyList<CoPurchaseRelated> Related);
