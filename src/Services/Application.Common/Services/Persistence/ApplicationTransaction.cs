using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Attributes;
using MassTransit;

namespace Application.Common.Services.Persistence;

public sealed class ApplicationTransaction(IApplicationTransactionContext context) : IApplicationTransaction
{
	private IApplicationTransactionContext _context = context;
	private TransactionalAttribute? _settings;
	private Action<TransactionError>? _onException;
	private bool _disposed;

	public IApplicationTransactionContext Context
	{
		get
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			return _context;
		}
	}

	public bool IsExecuting { get; private set; }

	internal void SetOwnedContext(OwnedApplicationTransactionContext ownedContext)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (IsExecuting || _context is OwnedApplicationTransactionContext)
			throw new InvalidOperationException("The transaction context has already been initialized or is executing.");
		_context = ownedContext;
	}

	public IApplicationTransaction WithSettings(TransactionalAttribute? settings)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (IsExecuting)
			throw new InvalidOperationException("Transaction settings cannot change during execution.");
		_settings = settings;
		return this;
	}

	public IApplicationTransaction OnException(Action<TransactionError> handler)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(handler);
		_onException += handler;
		return this;
	}

	public Task ExecuteAsync(
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);
		return ExecuteAsync(async (ctx, ct) =>
		{
			await action(ctx, ct);
			return true;
		}, cancellationToken);
	}

	public async Task<TResult> ExecuteAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(action);
		if (IsExecuting)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return await action(Context, cancellationToken);
		}

		IsExecuting = true;
		var handlers = _onException;
		Task<TResult> ExecuteAttemptAsync()
		{
			_onException = handlers;
			return ExecuteBoundaryAsync(action, cancellationToken);
		}

		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			return _settings is null
				? await ExecuteAttemptAsync()
				: await Context.UnitOfWork.ExecuteWithTransaction(
					_settings, ExecuteAttemptAsync, cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException &&
										!cancellationToken.IsCancellationRequested &&
										!Context.UnitOfWork.HasActiveTransaction)
		{
			_onException?.Invoke(Context.ExceptionClassifier.Classify(exception));
			throw;
		}
		finally
		{
			_settings = null;
			_onException = null;
			IsExecuting = false;
		}
	}

	private async Task<TResult> ExecuteBoundaryAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken)
	{
		var context = Context;
		try
		{
			var result = await context.DomainEventExecutor.ExecuteAsync(
				() => action(context, cancellationToken), cancellationToken);

			var events = context.IntegrationEventScope.Flush();
			foreach (var @event in events)
				if (@event.RoutingKey is null)
					await context.Publisher.Publish(@event.Message, cancellationToken);
				else
					await context.Publisher.Publish(@event.Message,
						pubContext => pubContext.SetRoutingKey(@event.RoutingKey), cancellationToken);

			if (events.Count != 0 && !context.UnitOfWork.Context.SuppressAutoSave)
				await context.UnitOfWork.SaveChangesAsync(cancellationToken);
			return result;
		}
		catch
		{
			context.IntegrationEventScope.Flush();
			throw;
		}
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		_settings = null;
		_onException = null;
		_context.Dispose();
	}

	public ValueTask DisposeAsync()
	{
		if (_disposed) return ValueTask.CompletedTask;
		_disposed = true;
		_settings = null;
		_onException = null;
		return _context.DisposeAsync();
	}
}
