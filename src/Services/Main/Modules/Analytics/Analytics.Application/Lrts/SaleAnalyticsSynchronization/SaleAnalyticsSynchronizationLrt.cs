using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Application.Analytics.Services;
using Main.Application.Lrts.Base.Synchronization;
using Main.Entities;
using Main.Modules.Sale.Abstractions;
using Microsoft.Extensions.Logging;

namespace Main.Application.Analytics.Lrts.SaleAnalyticsSynchronization;

public class SaleAnalyticsSynchronizationLrt(IApplicationTransactionFactory transactions,
	ILogger<SaleAnalyticsSynchronizationLrt> logger)
	: LrtBase<NoneInputState, SynchronizationState<Guid>>(transactions, logger)
{
	protected virtual int BatchSize => 500;
	public override string SystemName => nameof(SaleAnalyticsSynchronizationLrt);

	public override ILocalizableMessage NameLocalizationMessage
		=> LrtSaleAnalyticsSynchronizationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtSaleAnalyticsSynchronizationDescriptionMessage.Instance;

	protected override async Task DoWork(NoneInputState inputState)
	{
		while (true)
		{
			var processed = await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(20, 3),
				async (context, ct) =>
				{
					var page = await context.GetRequiredService<ISaleSnapshots>()
						.GetPageAsync(State.LastProcessedId, BatchSize, ct);
					if (page.SalesCount == 0) return 0;

					await context.GetRequiredService<SaleAnalyticsWriter>().WriteAsync(page.Contents, ct);
					await context.UnitOfWork.SaveChangesAsync(ct);
					await SaveStateAsync(
						new SynchronizationState<Guid>
						{
							LastProcessedId = page.LastSaleId,
							ProcessedCount = State.ProcessedCount + page.SalesCount
						});
					return page.SalesCount;
				},
				CancellationToken);
			if (processed < BatchSize) return;
		}
	}
}
