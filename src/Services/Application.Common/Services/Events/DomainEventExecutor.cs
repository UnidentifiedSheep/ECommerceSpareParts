using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using MediatR;
using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

public sealed class DomainEventExecutor(
	IDomainEventScope eventScope,
	IPublisher publisher,
	IDomainEventCollector collector,
	IUnitOfWork? unitOfWork = null) : IDomainEventExecutor
{
	private const int MaxDispatchRounds = 10;

	public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
	{
		await ExecuteAsync(
			async () =>
			{
				await action();
				return true;
			},
			cancellationToken);
	}

	public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);

		using var _ = eventScope.EnableCollection();

		try
		{
			var result = await action();
			await DispatchAsync(cancellationToken);
			return result;
		}
		catch
		{
			eventScope.Flush();
			throw;
		}
	}

	public async Task DispatchBeforeSaveAsync(CancellationToken cancellationToken = default)
	{
		using var collection = eventScope.EnableCollection();
		try
		{
			await DispatchAsync(cancellationToken, beforeSave: true);
		}
		catch
		{
			eventScope.Flush();
			throw;
		}
	}

	private async Task DispatchAsync(CancellationToken cancellationToken, bool beforeSave = false)
	{
		for (var round = 0; round < MaxDispatchRounds; round++)
		{
			if (beforeSave) collector.Collect();
			var events = beforeSave ? eventScope.FlushBeforeSave() : eventScope.Flush();
			if (events.Count == 0)
				return;

			foreach (var @event in events)
				await publisher.Publish((object)@event, cancellationToken);

			if (!beforeSave && unitOfWork is { Context.SuppressAutoSave: false })
				await unitOfWork.SaveChangesAsync(cancellationToken);
		}

		throw new InvalidOperationException($"Domain event dispatch exceeded {MaxDispatchRounds} rounds.");
	}
}
