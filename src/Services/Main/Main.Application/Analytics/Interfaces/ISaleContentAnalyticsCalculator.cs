using Main.Application.Analytics.Models;
using Main.Entities.Sale;

namespace Main.Application.Analytics.Interfaces;

public interface ISaleContentAnalyticsCalculator
{
	Task<SaleContentAnalyticsAmounts> CalculateAsync(
		SaleContent content,
		int currencyId,
		DateTime saleDate,
		CancellationToken cancellationToken);
}
