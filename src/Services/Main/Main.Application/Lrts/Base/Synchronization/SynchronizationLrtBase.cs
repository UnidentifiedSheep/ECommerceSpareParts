using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.Base.Synchronization;

public abstract class SynchronizationLrtBase<TId, TEvent>(
	IRepository<Job, Guid> jobRepository,
	IUnitOfWork unitOfWork,
	IPublishEndpoint publisher,
	IApplicationTransactionService transactionService,
	ILogger logger) : LrtBase<NoneInputState, SynchronizationState<TId>>(
	jobRepository, unitOfWork, publisher, transactionService, logger)
	where TId : struct
	where TEvent : class
{
	protected virtual int BatchSize => 1000;

	// A null cursor means that no items have been processed yet.
	protected abstract Task<SynchronizationPage<TId, TEvent>> GetPageAsync(
		TId? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken);

	protected override async Task DoWork(NoneInputState inputState)
	{
		while (true)
		{
			var page = await GetPageAsync(
				State.LastProcessedId,
				BatchSize,
				CancellationToken);

			if (page.Ids.Count == 0) break;
			if (page.Events.Count == 0)
				throw new InvalidOperationException("A nonempty synchronization page must contain events.");

			await TransactionService.ExecuteAsync(
				TransactionalAttribute.ReadCommitted(20, 2),
				async (context, cancellationToken) =>
				{
					foreach (var @event in page.Events)
						await Publisher.Publish(@event, cancellationToken);

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				},
				CancellationToken);

			await SaveStateAsync(new SynchronizationState<TId>
			{
				LastProcessedId = page.Ids[^1],
				ProcessedCount = State.ProcessedCount + page.Ids.Count
			});

			if (page.Ids.Count < BatchSize)
				break;
		}
	}
}

public sealed record SynchronizationPage<TId, TEvent>(
	IReadOnlyList<TId> Ids,
	IReadOnlyList<TEvent> Events);
