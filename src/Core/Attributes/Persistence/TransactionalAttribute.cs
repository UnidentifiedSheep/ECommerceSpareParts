using System.Data;

namespace Attributes;

public partial class TransactionalAttribute
{
    public static TransactionalAttribute RetryOnConflict(
        int delay,
        int retryCount)
    {
        return new TransactionalAttribute(
            IsolationLevel.ReadCommitted,
            delay,
            retryCount,
            "23505");
    }
}
