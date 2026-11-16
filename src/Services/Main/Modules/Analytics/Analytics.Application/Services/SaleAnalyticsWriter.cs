using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Analytics;
using Main.Modules.Sale.Abstractions;

namespace Main.Modules.Analytics.Application.Services;

public sealed class SaleAnalyticsWriter(
	SaleContentAnalyticsCalculator calculator,
	IRepository<SaleContentAnalytics, int> repository,
	IUnitOfWork unitOfWork)
{
	public async Task WriteAsync(
		IReadOnlyCollection<SaleContentSnapshot> snapshots,
		CancellationToken cancellationToken)
	{
		if (snapshots.Count == 0) return;
		if (!unitOfWork.HasActiveTransaction)
			throw new InvalidOperationException(
				"Sale analytics synchronization requires an active transaction.");

		var latest = snapshots
			.GroupBy(snapshot => snapshot.SaleContentId)
			.Select(group => group.Last())
			.ToArray();
		var ids = latest.Select(snapshot => snapshot.SaleContentId).ToArray();
		var existing = (await repository.ListAsync(
				Criteria<SaleContentAnalytics>.New()
					.Where(item => ids.Contains(item.SaleContentId))
					.Track()
					.Build(),
				cancellationToken))
			.ToDictionary(item => item.SaleContentId);

		foreach (var snapshot in latest)
		{
			existing.TryGetValue(snapshot.SaleContentId, out var row);
			if (!snapshot.IsCompleted)
			{
				row?.Deactivate();
				continue;
			}

			var amounts = await calculator.CalculateAsync(snapshot, cancellationToken);
			if (row is null)
				await unitOfWork.AddAsync(
					SaleContentAnalytics.Create(
						snapshot.SaleContentId,
						snapshot.SaleId,
						snapshot.OrganizationId,
						snapshot.BuyerId,
						snapshot.SaleDatetime,
						snapshot.Count,
						amounts.OriginalAmount,
						amounts.Revenue,
						amounts.Cost),
					cancellationToken);
			else
				row.Update(
					snapshot.SaleId,
					snapshot.OrganizationId,
					snapshot.BuyerId,
					snapshot.SaleDatetime,
					snapshot.Count,
					amounts.OriginalAmount,
					amounts.Revenue,
					amounts.Cost);
		}
	}
}
