using Microsoft.EntityFrameworkCore;

namespace Core.TransactionBuilder;

public static class DbContextTransactionExtensions
{
    public static TransactionConfig WithRetries(this DbContext context, int count) => 
        new TransactionConfig(context).WithRetries(count);
    public static TransactionConfig WithRetryDelay(this DbContext context, TimeSpan delay) => 
        new TransactionConfig(context).WithRetryDelay(delay);
    
    public static TransactionConfig WithIsolationLevel(this DbContext context, IsolationLevel isolationLevel) => 
        new TransactionConfig(context).WithIsolationLevel(isolationLevel);
    
    public static TransactionConfig WithDefaultTransactionSettings(this DbContext context, string variant) => 
        new TransactionConfig(context)
            .WithDefaultSettings(variant);
}