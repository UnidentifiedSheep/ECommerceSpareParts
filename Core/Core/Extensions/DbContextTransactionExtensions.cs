using Microsoft.EntityFrameworkCore;

namespace Core.Extensions;

public static class DbContextTransactionExtensions
{
    public static async Task WithTransactionAsync(this DbContext dbContext, Func<Task> action, CancellationToken cancellationToken = default)
    {
        var isLocalTransaction = dbContext.Database.CurrentTransaction == null;
        var transaction = dbContext.Database.CurrentTransaction ?? await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action();

            if (isLocalTransaction)
                await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (isLocalTransaction)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (isLocalTransaction)
                await transaction.DisposeAsync();
        }
    }
    
    public static async Task<T> WithTransactionAsync<T>(this DbContext dbContext, Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var isLocalTransaction = dbContext.Database.CurrentTransaction == null;
        var transaction = dbContext.Database.CurrentTransaction ?? await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            T result = await action();

            if (isLocalTransaction)
                await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            if (isLocalTransaction)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (isLocalTransaction)
                await transaction.DisposeAsync();
        }
    }

}