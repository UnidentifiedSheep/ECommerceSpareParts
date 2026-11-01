using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Application.Analytics.Interfaces;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Analytics.Lrts.SaleAnalyticsSynchronization;

public class SaleAnalyticsSynchronizationLrt(IApplicationTransactionFactory transactions,
	ILogger<SaleAnalyticsSynchronizationLrt> logger)
	: LrtBase<NoneInputState, SynchronizationState<Guid>>(transactions, logger)
{
	protected virtual int BatchSize => 500;
	public override string SystemName => nameof(SaleAnalyticsSynchronizationLrt);
	public override ILocalizableMessage NameLocalizationMessage => LrtSaleAnalyticsSynchronizationNameMessage.Instance;
	public override ILocalizableMessage DescriptionLocalizationMessage => LrtSaleAnalyticsSynchronizationDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		while (true)
		{
			var processed = await ExecuteInTransactionAsync(TransactionalAttribute.ReadCommitted(20, 3), async (context, ct) =>
			{
				var query = context.Repositories.GetForRead<Sale, Guid>().Query;
				if (State.LastProcessedId is { } lastId) query = query.Where(sale => sale.Id.CompareTo(lastId) > 0);
				var ids = await query.OrderBy(sale => sale.Id).Select(sale => sale.Id).Take(BatchSize).ToArrayAsync(ct);
				if (ids.Length == 0) return 0;

				await context.GetRequiredService<ISaleAnalyticsSynchronizer>().SynchronizeAsync(ids, ct);
				await context.UnitOfWork.SaveChangesAsync(ct);
				await SaveStateAsync(new SynchronizationState<Guid>
				{
					LastProcessedId = ids[^1], ProcessedCount = State.ProcessedCount + ids.Length
				});
				return ids.Length;
			}, CancellationToken);
			if (processed < BatchSize) return;
		}
	}
}
