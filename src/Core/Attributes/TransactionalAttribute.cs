using System.Data;

namespace Attributes;

[AttributeUsage(AttributeTargets.Class)]
public partial class TransactionalAttribute : Attribute
{
    private static readonly string[] DefaultPgqlErrors = ["40001", "40P01", "55P03"];

    /// <param name="isolationLevel">Уровень изоляции транзакции</param>
    /// <param name="retryDelayMs">Задержка перед повторением не удавшейся транзакции</param>
    /// <param name="retryCount">Количество повторных попыток</param>
    /// <param name="retryErrors">Ошибки при которых транзакция будет перезапускаться.</param>
    public TransactionalAttribute(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        int retryDelayMs = 0,
        int retryCount = 0,
        params string[] retryErrors)
    {
        IsolationLevel = isolationLevel;
        RetryDelayMs = retryDelayMs;
        RetryCount = retryCount;
        RetryErrors = retryErrors.Length == 0 
                ? DefaultPgqlErrors 
                : retryErrors.Union(DefaultPgqlErrors);
    }

    public IsolationLevel IsolationLevel { get; }
    public int RetryDelayMs { get; }
    public int RetryCount { get; }
    public IEnumerable<string> RetryErrors { get; }

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
