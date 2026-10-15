using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Attributes;

namespace Tests.Stubs;

internal sealed class ApplicationTransactionStub(IApplicationTransactionService service, TransactionalAttribute? settings)
	: IApplicationTransaction
{
	private Action<TransactionError>? _onException;

	public IApplicationTransaction OnException(Action<TransactionError> handler)
	{
		_onException += handler;
		return this;
	}

	public Task ExecuteAsync(Func<IApplicationTransactionContext, CancellationToken, Task> action, CancellationToken cancellationToken = default) =>
		ExecuteAsync(async (context, ct) =>
		{
			await action(context, ct);
			return true;
		}, cancellationToken);

	public async Task<TResult> ExecuteAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return await service.ExecuteAsync(settings, action, cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
		{
			_onException?.Invoke(new TransactionError(exception));
			throw;
		}
	}
}
