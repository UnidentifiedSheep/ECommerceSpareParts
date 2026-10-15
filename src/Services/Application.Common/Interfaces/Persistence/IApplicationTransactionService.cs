using Attributes;

namespace Application.Common.Interfaces.Persistence;

public interface IApplicationTransactionService
{
	IApplicationTransaction Create(TransactionalAttribute? settings = null);

	Task ExecuteAsync(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task> action,
		CancellationToken cancellationToken = default);

	Task<TResult> ExecuteAsync<TResult>(
		TransactionalAttribute? settings,
		Func<IApplicationTransactionContext, CancellationToken, Task<TResult>> action,
		CancellationToken cancellationToken = default);
}
