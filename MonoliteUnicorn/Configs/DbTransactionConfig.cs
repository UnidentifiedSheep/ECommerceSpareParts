using Core.TransactionBuilder;
using Microsoft.EntityFrameworkCore;

namespace MonoliteUnicorn.Configs;

public static class DbTransactionConfig
{
    public static void Configure()
    {
        HashSet<string> retryableErrors = ["40001", "40P01", "55P03"];
        TransactionConfig.AddDefaultSettings("normal-with-isolation", new TransactionDefaultSettings()
            .SetIsolationLevel(IsolationLevel.Serializable)
            .SetRetries(3)
            .SetPgErrorKeys(retryableErrors)
            .SetRetryDelay(TimeSpan.FromMilliseconds(100)));
        
        TransactionConfig.AddDefaultSettings("normal", new TransactionDefaultSettings()
            .SetIsolationLevel(IsolationLevel.ReadCommitted)
            .SetRetries(3)
            .SetPgErrorKeys(retryableErrors)
            .SetRetryDelay(TimeSpan.FromMilliseconds(100)));
        
        TransactionConfig.AddDefaultSettings("orchestrator-with-isolation", new TransactionDefaultSettings()
            .SetIsolationLevel(IsolationLevel.Serializable)
            .SetRetries(2)
            .SetPgErrorKeys(retryableErrors)
            .SetRetryDelay(TimeSpan.FromMilliseconds(100)));
    }
}