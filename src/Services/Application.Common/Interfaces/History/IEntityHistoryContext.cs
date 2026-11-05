using Domain;
using Domain.CommonEntities.History;

namespace Application.Common.Interfaces.History;

/// <summary>Stores entity change reasons for the current execution attempt.</summary>
public interface IEntityHistoryContext
{
	/// <summary>Adds a distinct reason for the entity identified by its typed key.</summary>
	/// <param name="id">The key of the changed entity, including composite keys.</param>
	/// <param name="reason">The reason and an optional reference to its source.</param>
	void AddReason<TEntity, TKey>(TKey id, EntityHistoryReason reason)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	/// <summary>Adds a reason using the entity's declared type and key.</summary>
	void AddReason<TEntity, TKey>(Entity<TEntity, TKey> entity, EntityHistoryReason reason)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	/// <summary>Returns reasons for the typed key, or an empty list if none are registered.</summary>
	IReadOnlyList<EntityHistoryReason> GetReasons<TEntity, TKey>(TKey id)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	/// <summary>Returns reasons for the entity, or an empty list if none are registered.</summary>
	IReadOnlyList<EntityHistoryReason> GetReasons<TEntity, TKey>(Entity<TEntity, TKey> entity)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull;

	/// <summary>Clears all reasons before a new attempt and after the operation completes.</summary>
	void Clear();
}
