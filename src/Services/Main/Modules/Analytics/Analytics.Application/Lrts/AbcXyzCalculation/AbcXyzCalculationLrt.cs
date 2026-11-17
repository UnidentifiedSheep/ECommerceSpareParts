using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Locan.Core.Interfaces;
using Main.Entities;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Interfaces;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Modules.Analytics.Application.Lrts.AbcXyzCalculation;

public class AbcXyzCalculationLrt(
	IApplicationTransactionFactory transactionService,
	ILogger<AbcXyzCalculationLrt> logger)
	: LrtBase<AbcXyzCalculationInputState, AbcXyzCalculationState>(transactionService, logger)
{
	protected virtual int BatchSize => 1000;

	public override string SystemName => nameof(AbcXyzCalculationLrt);

	public override ILocalizableMessage NameLocalizationMessage => LrtAbcXyzCalculationNameMessage.Instance;

	public override ILocalizableMessage DescriptionLocalizationMessage
		=> LrtAbcXyzCalculationDescriptionMessage.Instance;

	protected override async Task DoWork(AbcXyzCalculationInputState inputState)
	{
		var result = await ExecuteInScopeAsync(
			(context, ct) => context.GetRequiredService<IAbcXyzAnalyzer>().AnalyzeAsync(inputState, ct),
			CancellationToken);

		var draft = AbcXyzRunFactory.Create(inputState, result, JobId, Job.WhoCreated);

		await ExecuteInTransactionAsync(
			TransactionalAttribute.ReadCommitted(20, 2),
			async (context, ct) =>
			{
				var previous = await context.GetRequiredService<IReadRepository<AnalyticsRun, Guid>>()
					.Query
					.Where(run => run.JobId == JobId)
					.Select(run => run.Id)
					.ToListAsync(ct);
				if (previous.Count > 0)
					await context.Repositories.Get<AnalyticsRun, Guid>().DeleteManyAsync(previous, ct);

				await context.UnitOfWork.AddAsync(draft.Run, ct);
				await context.UnitOfWork.SaveChangesAsync(ct);

				foreach (var batch in draft.Items.Chunk(BatchSize))
				{
					await context.UnitOfWork.AddRangeAsync(batch, ct);
					await context.UnitOfWork.SaveChangesAsync(ct);
				}
			},
			CancellationToken);
	}
}
