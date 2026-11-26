using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Analytics.Application.Services;
using Main.Modules.Product.Abstractions;

namespace Main.Modules.Analytics.Application.Analyzers.CoPurchase;

public interface ICoPurchaseAnalyzer
{
	Task<IReadOnlyList<CoPurchaseProductResult>> AnalyzeAsync(
		IReadOnlyList<int> productIds,
		int totalSalesCount,
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken = default);
}

public sealed class CoPurchaseAnalyzer(
	ICoPurchaseDataReader reader,
	ICoPurchaseCalculator calculator,
	IProductCrosses productCrosses) : ICoPurchaseAnalyzer
{
	public async Task<IReadOnlyList<CoPurchaseProductResult>> AnalyzeAsync(
		IReadOnlyList<int> productIds,
		int totalSalesCount,
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken = default)
	{
		var crosses = new Dictionary<int, IReadOnlySet<int>>();
		await LoadCrossesAsync(productIds, crosses, cancellationToken);
		var salesByScope = new Dictionary<string, IReadOnlyList<CoPurchaseSaleProduct>>();
		var relatedByMembers = new Dictionary<string, IReadOnlyList<CoPurchaseRelated>>();
		var results = new List<CoPurchaseProductResult>(productIds.Count);

		foreach (var productId in productIds)
		{
			var crossIds = crosses[productId];
			var scope = crossIds.Append(productId).ToHashSet();
			var scopeKey = Key(scope);
			if (!salesByScope.TryGetValue(scopeKey, out var sales))
				salesByScope[scopeKey] = sales = await reader.GetSalesAsync(parameters, scope, cancellationToken);

			var candidates = calculator.SelectCandidates(productId, crossIds, sales, totalSalesCount, parameters);
			var membersKey = Key(candidates.Members);
			if (!relatedByMembers.TryGetValue(membersKey, out var related))
				relatedByMembers[membersKey] = related = await BuildRelatedAsync(
					candidates,
					crosses,
					totalSalesCount,
					parameters,
					cancellationToken);

			results.Add(new CoPurchaseProductResult(productId, candidates.SalesCount, related));
		}

		return results;
	}

	private async Task<IReadOnlyList<CoPurchaseRelated>> BuildRelatedAsync(
		CoPurchaseCandidates candidates,
		Dictionary<int, IReadOnlySet<int>> crosses,
		int totalSalesCount,
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken)
	{
		if (candidates.Candidates.Count == 0)
			return [];

		var candidateIds = candidates.Candidates.Select(candidate => candidate.ProductId).ToList();
		await LoadCrossesAsync(candidateIds, crosses, cancellationToken);

		var salesCounts = await reader.GetSalesCountsAsync(parameters, candidateIds, cancellationToken);
		return calculator.BuildRelated(candidates, crosses, salesCounts, totalSalesCount, parameters);
	}

	private async Task LoadCrossesAsync(
		IEnumerable<int> productIds,
		Dictionary<int, IReadOnlySet<int>> crosses,
		CancellationToken cancellationToken)
	{
		var missingIds = productIds.Where(productId => !crosses.ContainsKey(productId)).Distinct().ToList();
		if (missingIds.Count == 0)
			return;

		var loaded = await productCrosses.GetCrossIdsAsync(missingIds, cancellationToken);
		foreach (var productId in missingIds)
			crosses[productId] = loaded.TryGetValue(productId, out var crossIds)
				? crossIds.ToHashSet()
				: new HashSet<int>();
	}

	private static string Key(IEnumerable<int> productIds) => string.Join(',', productIds.Order());
}
