using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;

namespace Main.Modules.Analytics.Application.Interfaces;

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
