using System.Data;
using Enums;

namespace Persistence.Interfaces;

public interface ICustomTransaction
{
	ICustomTransaction WithRetries(int count);

	ICustomTransaction WithRetryDelay(TimeSpan delay);

	ICustomTransaction WithIsolationLevel(IsolationLevel isolationLevel);

	ICustomTransaction WithDefaultSettings(string variant);

	ICustomTransaction WithRetryOn(TransactionErrorKind errors);

	Task<T> ExecuteWithTransaction<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

	Task ExecuteWithTransaction(Func<Task> action, CancellationToken cancellationToken = default);
}
