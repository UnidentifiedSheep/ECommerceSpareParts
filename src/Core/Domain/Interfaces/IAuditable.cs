namespace Domain.Interfaces;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }

    /// <summary>
    /// Touches entity, updates updated at value
    /// </summary>
    void Touch();
}