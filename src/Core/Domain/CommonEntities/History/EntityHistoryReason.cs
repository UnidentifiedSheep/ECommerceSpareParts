using Domain.Keys;

namespace Domain.CommonEntities.History;

public sealed record EntityHistoryReason(string Code, string? SourceType = null, string? SourceKey = null)
{
	/// <summary>Creates a reason referencing a source entity by its typed key.</summary>
	public static EntityHistoryReason ForSource<TEntity, TKey>(string code, TKey id)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ArgumentNullException.ThrowIfNull(id);
		return new EntityHistoryReason(code, typeof(TEntity).Name, EntityKey.Format(id));
	}

	/// <summary>Creates a reason referencing the source entity.</summary>
	public static EntityHistoryReason ForSource<TEntity, TKey>(string code, Entity<TEntity, TKey> source)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(source);
		return ForSource<TEntity, TKey>(code, source.GetId());
	}
}
