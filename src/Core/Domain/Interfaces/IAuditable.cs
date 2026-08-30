namespace Domain.Interfaces;

public interface IAuditable
{
	DateTime CreatedAt { get; }

	DateTime UpdatedAt { get; }

	Guid? WhoCreated { get; }

	Guid? WhoUpdated { get; }

	/// <summary>
	///     Touches entity, updates updated at value
	/// </summary>
	void Touch(Guid? userId);

	void SetCreatedUser(Guid? userId);
}
