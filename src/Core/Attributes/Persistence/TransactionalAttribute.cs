using System.Data;
using Enums;

// ReSharper disable once CheckNamespace
namespace Attributes;

public partial class TransactionalAttribute
{
	public static TransactionalAttribute RetryOnConflict(int delay, int retryCount)
	{
		return new TransactionalAttribute(
			IsolationLevel.ReadCommitted,
			delay,
			retryCount,
			DefaultRetryErrors | TransactionErrorKind.UniqueConstraintViolation);
	}
}
