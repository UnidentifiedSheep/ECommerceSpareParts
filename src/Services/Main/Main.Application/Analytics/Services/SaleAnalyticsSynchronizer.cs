using Application.Common.Interfaces.Persistence;
using Main.Application.Analytics.Interfaces;
using Main.Entities.Sale;
using Main.Enums;

namespace Main.Application.Analytics.Services;

public sealed class SaleAnalyticsSynchronizer(
	ISaleContentAnalyticsCalculator calculator,
	IUnitOfWork unitOfWork) : ISaleAnalyticsSynchronizer
{
	public async Task SynchronizeAsync(IReadOnlyCollection<SaleContent> contents, CancellationToken cancellationToken)
	{
		if (contents.Count == 0) return;
		if (!unitOfWork.HasActiveTransaction)
			throw new InvalidOperationException("Sale analytics synchronization requires an active transaction.");

		foreach (var content in contents)
		{
			if (content.Sale is not { State: SaleState.Completed } sale || !sale.Contents.Contains(content)) continue;
			var amounts = await calculator.CalculateAsync(content, sale.CurrencyId, sale.SaleDatetime, cancellationToken);
			content.UpdateAnalytics(amounts.OriginalAmount, amounts.Revenue, amounts.Cost);
		}
	}
}
