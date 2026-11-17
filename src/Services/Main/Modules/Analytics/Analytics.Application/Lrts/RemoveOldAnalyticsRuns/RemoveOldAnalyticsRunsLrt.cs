using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Modules.Analytics.Application.Lrts.RemoveOldAnalyticsRuns;

public class RemoveOldAnalyticsRunsLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<RemoveOldAnalyticsRunsLrt> logger)
	: LrtBase<RemoveOldAnalyticsRunsInputState, RemoveOldAnalyticsRunsState>(transactionService, logger)
{
	public const string Name = nameof(RemoveOldAnalyticsRunsLrt);

	protected virtual int BatchSize => 500;

	public override string SystemName => Name;

	public override ILocalizableMessage NameLocalizationMessage => LrtRemoveOldAnalyticsRunsNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtRemoveOldAnalyticsRunsDescriptionMessage.Instance;

	protected override async Task DoWork(RemoveOldAnalyticsRunsInputState inputState)
	{
		var createdBefore = DateTime.UtcNow.AddDays(-inputState.OlderThanDays);

		while (true)
		{
			var batch = await ExecuteInScopeAsync(
				(context, ct) => context.GetRequiredService<IReadRepository<AnalyticsRun, Guid>>()
					.Query
					.Where(run => !run.IsPinned && run.CreatedAt < createdBefore)
					.OrderBy(run => run.CreatedAt)
					.Select(run => run.Id)
					.Take(BatchSize)
					.ToListAsync(ct),
				CancellationToken);

			if (batch.Count == 0) return;

			await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(30, 3),
				async (context, ct) => await context.Repositories
					.Get<AnalyticsRun, Guid>()
					.DeleteManyAsync(batch, ct),
				CancellationToken);
		}
	}
}
