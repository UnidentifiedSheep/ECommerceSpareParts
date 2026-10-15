using Application.Common.Interfaces.Persistence;
using Application.Common.Models.Persistence;
using Attributes;

namespace Application.Common.Services.Persistence;

internal sealed class ApplicationTransaction(
	ApplicationTransactionService service,
	IUnitOfWork unitOfWork,
	ITransactionExceptionClassifier exceptionClassifier,
	TransactionalAttribute? settings) : IApplicationTransaction
{
	private Action<TransactionError>? _onException;

	public IApplicationTransaction OnException(Action<TransactionError> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);
		_onException += handler;
		return this;
	}

	public Task ExecuteAsync(
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);
		return ExecuteAsync(async (context, ct) =>
		{
			await action(context, ct);
			return true;
		}, cancellationToken);
	}

	public async Task<TResult> ExecuteAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);
		var onException = _onException;
		try
		{
			return await service.ExecuteTransactionAsync(settings, action, cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException &&
		                                !cancellationToken.IsCancellationRequested &&
		                                !unitOfWork.HasActiveTransaction)
		{
			onException?.Invoke(exceptionClassifier.Classify(exception));
			throw;
		}
	}
}
