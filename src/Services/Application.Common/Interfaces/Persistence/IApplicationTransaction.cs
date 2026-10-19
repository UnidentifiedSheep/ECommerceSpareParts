using Application.Common.Models.Persistence;
using Attributes;

namespace Application.Common.Interfaces.Persistence;

public interface IApplicationTransaction : IDisposable, IAsyncDisposable
{
	IApplicationTransactionContext Context { get; }

	bool IsExecuting { get; }

	IApplicationTransaction WithSettings(TransactionalAttribute? settings);

	/// <summary>
	/// Registers a handler for a failed operation after rollback and retries.
	/// Enclosing database transactions defer error handling to their owner.
	/// Returning from the handler does not suppress the original exception.
	/// </summary>
	IApplicationTransaction OnException(Action<TransactionError> handler);

	Task ExecuteAsync(
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default);

	Task<TResult> ExecuteAsync<TResult>(
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default);
}
