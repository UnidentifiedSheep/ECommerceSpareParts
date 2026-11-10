using Application.Common.Interfaces.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Domain.Interfaces.Events;

namespace Persistence.Interceptors;

public class DomainEventFlusherSaveChangesInterceptor(
	IDomainEventCollector collector,
	Func<IDomainEventExecutor> executorFactory) : SaveChangesInterceptor
{
	private bool _saving;

	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData, InterceptionResult<int> result) =>
		throw new NotSupportedException("Use SaveChangesAsync instead of SaveChanges.");

	public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
	{
		if (eventData.Context is not { } context) return result;
		EnterSave();
		try
		{
			await executorFactory().DispatchBeforeSaveAsync(cancellationToken);
			return result;
		}
		catch { Reset(context); throw; }
	}

	public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
	{
		Reset(eventData.Context);
		return result;
	}

	public override ValueTask<int> SavedChangesAsync(
		SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
	{
		Reset(eventData.Context);
		return ValueTask.FromResult(result);
	}

	public override void SaveChangesFailed(DbContextErrorEventData eventData) => Reset(eventData.Context);

	public override Task SaveChangesFailedAsync(
		DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
	{
		Reset(eventData.Context);
		return Task.CompletedTask;
	}

	public override void SaveChangesCanceled(DbContextEventData eventData) => Reset(eventData.Context);

	public override Task SaveChangesCanceledAsync(
		DbContextEventData eventData, CancellationToken cancellationToken = default)
	{
		Reset(eventData.Context);
		return Task.CompletedTask;
	}

	private void EnterSave()
	{
		if (_saving)
			throw new InvalidOperationException("Before-save handlers must not call SaveChanges on the same DbContext.");
		_saving = true;
	}

	private void Reset(DbContext? context)
	{
		if (context is null) return;
		_saving = false;
		collector.Reset();
	}
}
