using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Attributes;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.Base.Synchronization;

public abstract class SynchronizationLrtBase<TInputState, TId, TEvent>(
	IApplicationTransactionFactory transactionService,
	ILogger logger) : LrtBase<TInputState, SynchronizationState<TId>>(transactionService, logger)
	where TInputState : class, IInputState
	where TId : struct
	where TEvent : class
{
	protected virtual int BatchSize => 1000;

	// A null cursor means that no items have been processed yet.
	protected abstract Task<SynchronizationPage<TId, TEvent>> GetPageAsync(
		TId? lastProcessedId,
		int batchSize,
		CancellationToken cancellationToken);

	protected override async Task DoWork(TInputState inputState)
	{
		while (true)
		{
			var processed = await ExecuteInTransactionAsync(
				TransactionalAttribute.ReadCommitted(20, 2),
				async (context, cancellationToken) =>
				{
					var page = await GetPageAsync(State.LastProcessedId, BatchSize, cancellationToken);
					if (page.Ids.Count == 0) return 0;
					if (page.Events.Count == 0)
						throw new InvalidOperationException("A nonempty synchronization page must contain events.");

					foreach (var @event in page.Events)
						await context.Publisher.Publish(@event, @event.GetType(), cancellationToken);

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
					await SaveStateAsync(new SynchronizationState<TId>
					{
						LastProcessedId = page.Ids[^1],
						ProcessedCount = State.ProcessedCount + page.Ids.Count
					});
					return page.Ids.Count;
				}, CancellationToken);

			if (processed < BatchSize) break;
		}
	}
}

public sealed record SynchronizationPage<TId, TEvent>(
	IReadOnlyList<TId> Ids,
	IReadOnlyList<TEvent> Events);
