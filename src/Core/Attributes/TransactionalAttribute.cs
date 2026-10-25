using System.Data;
using Enums;

namespace Attributes;

[AttributeUsage(AttributeTargets.Class)]
public partial class TransactionalAttribute : Attribute
{
	public const TransactionErrorKind DefaultRetryErrors =
		TransactionErrorKind.ConcurrencyConflict | TransactionErrorKind.Deadlock | TransactionErrorKind.LockUnavailable;

	/// <param name="isolationLevel">Уровень изоляции транзакции</param>
	/// <param name="retryDelayMs">Задержка перед повторением не удавшейся транзакции</param>
	/// <param name="retryCount">Количество повторных попыток</param>
	/// <param name="retryErrors">Дополнительные ошибки для повторения транзакции. Стандартные ошибки включаются всегда.</param>
	public TransactionalAttribute(
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
		int retryDelayMs = 0,
		int retryCount = 0,
		TransactionErrorKind retryErrors = TransactionErrorKind.None)
	{
		IsolationLevel = isolationLevel;
		RetryDelayMs = retryDelayMs;
		RetryCount = retryCount;
		RetryErrors = DefaultRetryErrors | retryErrors;
	}

	public IsolationLevel IsolationLevel { get; }

	public int RetryDelayMs { get; }

	public int RetryCount { get; }

	public TransactionErrorKind RetryErrors { get; }

	public static TransactionalAttribute ReadCommitted(int delay, int retryCount)
	{
		return new TransactionalAttribute(
			IsolationLevel.ReadCommitted,
			delay,
			retryCount);
	}

	public static TransactionalAttribute Serializable(int delay, int retryCount)
	{
		return new TransactionalAttribute(
			IsolationLevel.Serializable,
			delay,
			retryCount);
	}
}
