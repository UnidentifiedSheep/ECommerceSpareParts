using Abstractions.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptor(IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var modified = context.ChangeTracker
            .Entries<IAuditable>()
            .Where(x => x.State is EntityState.Modified or EntityState.Added);

        foreach (var entry in modified)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedUser(userContext.UserId);
            entry.Entity.Touch(userContext.UserId);
        }
        
    }
}