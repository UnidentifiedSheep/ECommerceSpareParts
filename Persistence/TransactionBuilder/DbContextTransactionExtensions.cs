using System.Data;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.TransactionBuilder;

public static class DbContextTransactionExtensions
{
    public static ICustomTransaction WithRetries(this DbContext context, int count) => 
        new TransactionConfig(context).WithRetries(count);
    public static ICustomTransaction WithRetryDelay(this DbContext context, TimeSpan delay) => 
        new TransactionConfig(context).WithRetryDelay(delay);
    
    public static ICustomTransaction WithIsolationLevel(this DbContext context, IsolationLevel isolationLevel) => 
        new TransactionConfig(context).WithIsolationLevel(isolationLevel);
    
    public static ICustomTransaction WithDefaultTransactionSettings(this DbContext context, string variant) => 
        new TransactionConfig(context)
            .WithDefaultSettings(variant);
}