using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;

namespace Main.Modules.Analytics.Application.Analyzers.CoPurchase;

public interface ICoPurchaseCalculator
{
	CoPurchaseCandidates SelectCandidates(
		int productId,
		IReadOnlySet<int> crossIds,
		IReadOnlyCollection<CoPurchaseSaleProduct> sales,
		int totalSalesCount,
		CoPurchaseParameters parameters);

	IReadOnlyList<CoPurchaseRelated> BuildRelated(
		CoPurchaseCandidates candidates,
		IReadOnlyDictionary<int, IReadOnlySet<int>> candidateCrosses,
		IReadOnlyDictionary<int, int> productSalesCounts,
		int totalSalesCount,
		CoPurchaseParameters parameters);
}

public sealed class CoPurchaseCalculator : ICoPurchaseCalculator
{
	private const int Precision = 6;

	public CoPurchaseCandidates SelectCandidates(
		int productId,
		IReadOnlySet<int> crossIds,
		IReadOnlyCollection<CoPurchaseSaleProduct> sales,
		int totalSalesCount,
		CoPurchaseParameters parameters)
	{
		var productsBySale = sales
			.GroupBy(row => row.SaleId)
			.ToDictionary(
				group => group.Key,
				group => group.Select(row => row.ProductId).ToHashSet());

		var salesCountByProduct = new Dictionary<int, int>();
		foreach (var products in productsBySale.Values)
		foreach (var id in products)
			salesCountByProduct[id] = salesCountByProduct.GetValueOrDefault(id) + 1;

		var productSales = productsBySale.Values.Where(products => products.Contains(productId)).ToList();

		var members = new HashSet<int> { productId };
		foreach (var crossId in crossIds)
			if (crossId != productId &&
				!IsSuspiciousCross(crossId, productSales, salesCountByProduct, totalSalesCount, parameters))
				members.Add(crossId);

		var pooledSales = productsBySale.Where(sale => sale.Value.Overlaps(members)).ToList();

		var candidateSales = new Dictionary<int, HashSet<Guid>>();
		foreach (var (saleId, products) in pooledSales)
		foreach (var id in products)
		{
			if (members.Contains(id))
				continue;

			if (!candidateSales.TryGetValue(id, out var saleIds))
				candidateSales[id] = saleIds = [];
			saleIds.Add(saleId);
		}

		var candidates = candidateSales
			.OrderByDescending(candidate => candidate.Value.Count)
			.ThenBy(candidate => candidate.Key)
			.Take(parameters.CandidatesTopK)
			.Select(candidate => new CoPurchaseCandidate(candidate.Key, candidate.Value))
			.ToList();

		return new CoPurchaseCandidates(productId, members, pooledSales.Count, candidates);
	}

	public IReadOnlyList<CoPurchaseRelated> BuildRelated(
		CoPurchaseCandidates candidates,
		IReadOnlyDictionary<int, IReadOnlySet<int>> candidateCrosses,
		IReadOnlyDictionary<int, int> productSalesCounts,
		int totalSalesCount,
		CoPurchaseParameters parameters)
	{
		if (candidates.SalesCount == 0 || totalSalesCount == 0)
			return [];

		var related = new List<CoPurchaseRelated>();
		foreach (var group in GroupByCrosses(candidates.Candidates, candidateCrosses))
		{
			var pairCount = group.SelectMany(candidate => candidate.SaleIds).Distinct().Count();
			if (pairCount < parameters.MinPairCount)
				continue;

			var relatedSalesCount = Math.Max(
				pairCount,
				group.Sum(candidate =>
					productSalesCounts.GetValueOrDefault(candidate.ProductId, candidate.SaleIds.Count)));

			var lift = Math.Round(
				(decimal)pairCount * totalSalesCount / ((decimal)candidates.SalesCount * relatedSalesCount),
				Precision);
			if (lift <= 1)
				continue;

			var members = group
				.OrderByDescending(candidate => candidate.SaleIds.Count)
				.ThenBy(candidate => candidate.ProductId)
				.Select(candidate => new CoPurchaseRelatedMember(candidate.ProductId, candidate.SaleIds.Count))
				.ToList();

			related.Add(new CoPurchaseRelated(
				members,
				pairCount,
				Math.Round((decimal)pairCount / candidates.SalesCount, Precision),
				lift));
		}

		return related
			.OrderByDescending(item => item.Confidence)
			.ThenByDescending(item => item.Lift)
			.ThenBy(item => item.Members[0].ProductId)
			.Take(parameters.ResultTopN)
			.ToList();
	}

	private static bool IsSuspiciousCross(
		int crossId,
		IReadOnlyCollection<HashSet<int>> productSales,
		IReadOnlyDictionary<int, int> salesCountByProduct,
		int totalSalesCount,
		CoPurchaseParameters parameters)
	{
		var crossSalesCount = salesCountByProduct.GetValueOrDefault(crossId);
		if (productSales.Count == 0 || crossSalesCount == 0 || totalSalesCount == 0)
			return false;

		var pairCount = productSales.Count(products => products.Contains(crossId));
		if (pairCount < parameters.SuspiciousCrossMinPairCount)
			return false;

		var lift = (decimal)pairCount * totalSalesCount / ((decimal)productSales.Count * crossSalesCount);
		return lift >= parameters.SuspiciousCrossMinLift;
	}

	private static List<List<CoPurchaseCandidate>> GroupByCrosses(
		IReadOnlyList<CoPurchaseCandidate> candidates,
		IReadOnlyDictionary<int, IReadOnlySet<int>> candidateCrosses)
	{
		var assigned = new HashSet<int>();
		var groups = new List<List<CoPurchaseCandidate>>();
		foreach (var candidate in candidates)
		{
			if (!assigned.Add(candidate.ProductId))
				continue;

			var group = new List<CoPurchaseCandidate> { candidate };
			foreach (var other in candidates)
				if (!assigned.Contains(other.ProductId) && AreCrosses(candidate.ProductId, other.ProductId, candidateCrosses))
				{
					assigned.Add(other.ProductId);
					group.Add(other);
				}

			groups.Add(group);
		}

		return groups;
	}

	private static bool AreCrosses(int left, int right, IReadOnlyDictionary<int, IReadOnlySet<int>> crosses)
		=> crosses.TryGetValue(left, out var leftCrosses) && leftCrosses.Contains(right) ||
			crosses.TryGetValue(right, out var rightCrosses) && rightCrosses.Contains(left);
}
