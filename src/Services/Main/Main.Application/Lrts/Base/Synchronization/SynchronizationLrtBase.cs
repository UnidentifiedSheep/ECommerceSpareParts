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

	// Items must be ordered by ID. A null cursor means that no items have been processed yet.
	protected abstract Task<IReadOnlyList<SynchronizationEvent<TId, TEvent>>> GetEventsAsync(
		TId? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken);

	protected override async Task DoWork(NoneInputState inputState)
	{
		while (true)
		{
			var events = await GetEventsAsync(
				State.LastProcessedId,
				BatchSize,
				CancellationToken);

			if (events.Count == 0) break;

			await TransactionService.ExecuteAsync(
				TransactionalAttribute.ReadCommitted(20, 2),
				async (context, cancellationToken) =>
				{
					foreach (var item in events)
						await Publisher.Publish(item.Event, cancellationToken);

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
				},
				CancellationToken);

			await SaveStateAsync(new SynchronizationState<TId>
			{
				LastProcessedId = events[^1].Id,
				ProcessedCount = State.ProcessedCount + events.Count
			});

			if (events.Count < BatchSize)
				break;
		}
	}
}

public sealed record SynchronizationEvent<TId, TEvent>(TId Id, TEvent Event);
