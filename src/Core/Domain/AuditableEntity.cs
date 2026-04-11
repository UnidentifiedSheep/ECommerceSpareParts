using Domain.Interfaces;

namespace Domain;

public abstract class AuditableEntity<TModel, TKey> : Entity<TModel, TKey>, IAuditable
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}