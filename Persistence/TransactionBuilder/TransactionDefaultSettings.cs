using System.Data;

namespace Persistence.TransactionBuilder;

public class TransactionDefaultSettings
{
    public HashSet<string> RetryOn { get; } = [];

    public int RetriesCount { get; private set; }
    public TimeSpan RetryDelay { get; private set; } = TimeSpan.Zero;
    public IsolationLevel IsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;

    public TransactionDefaultSettings SetRetries(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative");
        RetriesCount = count;
        return this;
    }

    public TransactionDefaultSettings SetRetryDelay(TimeSpan delay)
    {
        RetryDelay = delay;
        return this;
    }

    public TransactionDefaultSettings SetIsolationLevel(IsolationLevel isolationLevel)
    {
        if (isolationLevel == IsolationLevel.Unspecified)
            throw new ArgumentException("Уровень изоляции не должен быть 'Unspecified'");
        IsolationLevel = isolationLevel;
        return this;
    }

    public TransactionDefaultSettings SetPgErrorKeys(IEnumerable<string> keys)
    {
        RetryOn.Clear();
        RetryOn.UnionWith(keys);
        return this;
    }
}