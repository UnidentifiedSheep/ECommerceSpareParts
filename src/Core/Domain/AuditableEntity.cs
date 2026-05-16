using Domain.Interfaces;

namespace Domain;

public abstract class AuditableEntity<TModel, TKey>
    : Entity<TModel, TKey>, IAuditable
    where TModel : Entity<TModel, TKey> where TKey : notnull
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Guid? WhoCreated { get; private set; }
    public Guid? WhoUpdated { get; private set; }

    public void SetCreatedUser(Guid? userId)
    {
        if (WhoUpdated != null)
            throw new InvalidOperationException("Can't set created user, it's already set");

        if (WhoCreated == Guid.Empty)
            throw new InvalidOperationException("Can't set empty created user id.");

        WhoCreated = userId;
    }

    public void Touch(Guid? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        WhoUpdated = userId;
    }
}