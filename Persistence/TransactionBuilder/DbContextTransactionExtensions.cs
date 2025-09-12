using System.Data;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.TransactionBuilder;

public static class DbContextTransactionExtensions
{
    public static ICustomTransaction WithRetries(this DbContext context, int count)
    {
        return new TransactionConfig(context).WithRetries(count);
    }

    public static ICustomTransaction WithRetryDelay(this DbContext context, TimeSpan delay)
    {
        return new TransactionConfig(context).WithRetryDelay(delay);
    }

    public static ICustomTransaction WithIsolationLevel(this DbContext context, IsolationLevel isolationLevel)
    {
        return new TransactionConfig(context).WithIsolationLevel(isolationLevel);
    }

    public static ICustomTransaction WithDefaultTransactionSettings(this DbContext context, string variant)
    {
        return new TransactionConfig(context)
            .WithDefaultSettings(variant);
    }
}