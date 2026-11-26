namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseRelatedLookup(
	Guid RunId,
	int SourceProductId,
	IReadOnlyList<CoPurchaseRelated> Related);
