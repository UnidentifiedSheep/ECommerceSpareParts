namespace Main.Modules.Analytics.Application.Models.CoPurchase;

public sealed record CoPurchaseCandidates(
	int ProductId,
	IReadOnlySet<int> Members,
	int SalesCount,
	IReadOnlyList<CoPurchaseCandidate> Candidates);
