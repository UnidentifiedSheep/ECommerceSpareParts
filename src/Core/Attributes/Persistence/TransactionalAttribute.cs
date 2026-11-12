using System.Data;
using Enums;

// ReSharper disable once CheckNamespace
namespace Attributes;

public partial class TransactionalAttribute
{
	public static TransactionalAttribute RetryOnConflict(int delay, int retryCount)
		=> new(
			IsolationLevel.ReadCommitted,
			delay,
			retryCount,
			TransactionErrorKind.UniqueConstraintViolation);
}
