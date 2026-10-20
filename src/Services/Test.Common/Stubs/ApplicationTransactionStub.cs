using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Attributes;

namespace Tests.Stubs;

public class ApplicationTransactionStub(IApplicationTransactionContext context) : IApplicationTransaction
{
	private Action<TransactionError>? _onException;
	public IApplicationTransactionContext Context => context;
	public bool IsExecuting { get; private set; }
	public TransactionalAttribute? Settings { get; private set; }
	public int Calls { get; private set; }

	public IApplicationTransaction WithSettings(TransactionalAttribute? settings)
	{
		if (IsExecuting) throw new InvalidOperationException("Transaction settings cannot change during execution.");
		Settings = settings;
		return this;
	}

	public IApplicationTransaction OnException(Action<TransactionError> handler)
	{
		_onException += handler;
		return this;
	}

	public Task ExecuteAsync(Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default) => ExecuteAsync(async (ctx, ct) =>
		{
			await action(ctx, ct);
			return true;
		}, cancellationToken);

	public async Task<TResult> ExecuteAsync<TResult>(Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		Calls++;
		IsExecuting = true;
		try
		{
			return await action(context, cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
		{
			_onException?.Invoke(new TransactionError(exception));
			throw;
		}
		finally
		{
			IsExecuting = false;
			_onException = null;
		}
	}

	public void Dispose() { }

	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
