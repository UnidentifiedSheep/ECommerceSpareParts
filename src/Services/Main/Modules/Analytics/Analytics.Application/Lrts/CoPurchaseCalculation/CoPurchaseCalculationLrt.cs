using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Analyzers.CoPurchase;
using Main.Modules.Analytics.Application.Services;
using Microsoft.Extensions.Logging;

namespace Main.Modules.Analytics.Application.Lrts.CoPurchaseCalculation;

public class CoPurchaseCalculationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<CoPurchaseCalculationLrt> logger)
	: LrtBase<CoPurchaseCalculationInputState, CoPurchaseCalculationState>(transactionService, logger)
{
	protected virtual int BatchSize => 50;
	protected override TimeSpan LeaseDuration => TimeSpan.FromMinutes(10);

	public override string SystemName => nameof(CoPurchaseCalculationLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtCoPurchaseCalculationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtCoPurchaseCalculationDescriptionMessage.Instance;

	protected override async Task DoWork(CoPurchaseCalculationInputState inputState)
	{
		if (State.RunId is null)
			await StartRunAsync(inputState);

		int processed;
		do
			processed = await ProcessBatchAsync(inputState);
		while (processed == BatchSize);

		await CompleteRunAsync();
	}

	private Task StartRunAsync(CoPurchaseCalculationInputState inputState)
		=> ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, ct) =>
			{
				var totalSalesCount = await context.GetRequiredService<ICoPurchaseDataReader>()
					.GetTotalSalesCountAsync(inputState, ct);
				var run = CoPurchaseRunFactory.CreateRun(inputState, JobId, Job.WhoCreated);
				await context.UnitOfWork.AddAsync(run, ct);
				await context.UnitOfWork.SaveChangesAsync(ct);
				await SaveStateAsync(State with { RunId = run.Id, TotalSalesCount = totalSalesCount });
			},
			CancellationToken);

	private Task<int> ProcessBatchAsync(CoPurchaseCalculationInputState inputState)
		=> ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, ct) =>
			{
				var productIds = await context.GetRequiredService<ICoPurchaseDataReader>()
					.GetSoldProductIdsAsync(inputState, State.LastProductId, BatchSize, ct);
				if (productIds.Count == 0)
					return 0;

				var results = await context.GetRequiredService<ICoPurchaseAnalyzer>()
					.AnalyzeAsync(productIds, State.TotalSalesCount, inputState, ct);
				var runId = State.RunId!.Value;
				await context.UnitOfWork.AddRangeAsync(
					results.Select(result => CoPurchaseRunFactory.CreateItem(runId, result)),
					ct);
				await context.UnitOfWork.SaveChangesAsync(ct);
				await SaveStateAsync(
					State with
					{
						LastProductId = productIds[^1],
						ProductsCount = State.ProductsCount + results.Count,
						ProductsWithRelatedCount =
						State.ProductsWithRelatedCount + results.Count(result => result.Related.Count > 0)
					});
				return productIds.Count;
			},
			CancellationToken);

	private Task CompleteRunAsync()
		=> ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, ct) =>
			{
				var run = await context.Repositories.Get<AnalyticsRun, Guid>().GetById(State.RunId!.Value, ct) ??
					throw new InvalidOperationException($"Analytics run {State.RunId} not found.");
				if (run.CompletedAt is not null)
					return;

				run.Complete(
					CoPurchaseRunFactory.SerializeSummary(
						new CoPurchaseSummary(
							State.ProductsCount,
							State.ProductsWithRelatedCount,
							State.TotalSalesCount)),
					State.ProductsCount);
				await context.UnitOfWork.SaveChangesAsync(ct);
			},
			CancellationToken);
}
