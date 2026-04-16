namespace Domain.Interfaces;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    public Guid WhoCreated { get; }
    public Guid? WhoUpdated { get; }

    /// <summary>
    /// Touches entity, updates updated at value
    /// </summary>
    void Touch(Guid userId);
    void SetCreatedUser(Guid userId);
}