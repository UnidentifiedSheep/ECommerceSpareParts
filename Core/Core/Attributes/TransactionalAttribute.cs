using System.Data;

namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TransactionalAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; }
    public int RetryDelayMs { get; }
    public int RetryCount { get; }
    public IEnumerable<string> RetryErrors { get; }
    
    /// <param name="isolationLevel">Уровень изоляции транзакции</param>
    /// <param name="retryDelayMs">Задержка перед повторением не удавшейся транзакции</param>
    /// <param name="retryCount">Количество повторных попыток</param>
    /// <param name="retryErrors">Ошибки при которых транзакция будет перезапускаться.</param>
    public TransactionalAttribute(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int retryDelayMs = 0, 
        int retryCount = 0, string[]? retryErrors = null)
    {
        IsolationLevel = isolationLevel;
        RetryDelayMs = retryDelayMs;
        RetryCount = retryCount;
        RetryErrors = retryErrors ?? ["40001", "40P01", "55P03"];
    }
}