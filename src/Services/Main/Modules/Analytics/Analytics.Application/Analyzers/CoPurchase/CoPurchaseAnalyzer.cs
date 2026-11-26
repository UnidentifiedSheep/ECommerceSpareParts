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

		var selected = await SelectCandidatesAsync(productIds, crosses, totalSalesCount, parameters, cancellationToken);

		var candidateIds = selected
			.SelectMany(candidates => candidates.Candidates)
			.Select(candidate => candidate.ProductId)
			.ToHashSet();
		if (candidateIds.Count == 0)
			return selected
				.Select(candidates => new CoPurchaseProductResult(candidates.ProductId, candidates.SalesCount, []))
				.ToList();

		await LoadCrossesAsync(candidateIds, crosses, cancellationToken);
		var salesCounts = await reader.GetSalesCountsAsync(parameters, candidateIds, cancellationToken);

		var relatedByMembers = new Dictionary<string, IReadOnlyList<CoPurchaseRelated>>();
		var results = new List<CoPurchaseProductResult>(selected.Count);
		foreach (var candidates in selected)
		{
			var membersKey = Key(candidates.Members);
			if (!relatedByMembers.TryGetValue(membersKey, out var related))
				relatedByMembers[membersKey] = related =
					calculator.BuildRelated(candidates, crosses, salesCounts, totalSalesCount, parameters);

			results.Add(new CoPurchaseProductResult(candidates.ProductId, candidates.SalesCount, related));
		}

		return results;
	}

	private async Task<List<CoPurchaseCandidates>> SelectCandidatesAsync(
		IReadOnlyList<int> productIds,
		IReadOnlyDictionary<int, IReadOnlySet<int>> crosses,
		int totalSalesCount,
		CoPurchaseParameters parameters,
		CancellationToken cancellationToken)
	{
		var salesByScope = new Dictionary<string, IReadOnlyList<CoPurchaseSaleProduct>>();
		var selected = new List<CoPurchaseCandidates>(productIds.Count);
		foreach (var productId in productIds)
		{
			var crossIds = crosses[productId];
			var scope = crossIds.Append(productId).ToHashSet();
			var scopeKey = Key(scope);
			if (!salesByScope.TryGetValue(scopeKey, out var sales))
				salesByScope[scopeKey] = sales = await reader.GetSalesAsync(parameters, scope, cancellationToken);

			selected.Add(calculator.SelectCandidates(productId, crossIds, sales, totalSalesCount, parameters));
		}

		return selected;
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
