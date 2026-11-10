using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence.Interfaces;

namespace Persistence.Interceptors;

public class DomainEventFlusherSaveChangesInterceptor(IDomainEventCollector collector) : SaveChangesInterceptor
{
	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData, InterceptionResult<int> result)
	{
		Collect(eventData.Context);
		return result;
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
	{
		Collect(eventData.Context);
		return ValueTask.FromResult(result);
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

	private void Collect(DbContext? context)
	{
		try { collector.Collect(context); }
		catch
		{
			Reset(context);
			throw;
		}
	}

	private void Reset(DbContext? context)
	{
		if (context is not null) collector.Reset(context);
	}
}
