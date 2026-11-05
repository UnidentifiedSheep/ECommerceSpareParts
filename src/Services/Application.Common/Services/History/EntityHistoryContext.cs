using Application.Common.Interfaces.History;
using Domain;
using Domain.CommonEntities.History;
using Domain.Keys;

namespace Application.Common.Services.History;

public sealed class EntityHistoryContext : IEntityHistoryContext
{
	private readonly Dictionary<(Type EntityType, string EntityKey), HashSet<EntityHistoryReason>> _reasons = [];

	public void AddReason<TEntity, TKey>(TKey id, EntityHistoryReason reason)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(id);
		ArgumentNullException.ThrowIfNull(reason);
		ArgumentException.ThrowIfNullOrWhiteSpace(reason.Code);
		if (reason.SourceType is not null || reason.SourceKey is not null)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(reason.SourceType);
			ArgumentException.ThrowIfNullOrWhiteSpace(reason.SourceKey);
		}

		var key = (typeof(TEntity), EntityKey.Format(id));
		if (!_reasons.TryGetValue(key, out var reasons))
			_reasons.Add(key, reasons = []);
		reasons.Add(reason);
	}

	public void AddReason<TEntity, TKey>(Entity<TEntity, TKey> entity, EntityHistoryReason reason)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(entity);
		AddReason<TEntity, TKey>(entity.GetId(), reason);
	}

	public IReadOnlyList<EntityHistoryReason> GetReasons<TEntity, TKey>(TKey id)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(id);
		return _reasons.TryGetValue((typeof(TEntity), EntityKey.Format(id)), out var reasons) ? reasons.ToArray() : [];
	}

	public IReadOnlyList<EntityHistoryReason> GetReasons<TEntity, TKey>(Entity<TEntity, TKey> entity)
		where TEntity : Entity<TEntity, TKey> where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(entity);
		return GetReasons<TEntity, TKey>(entity.GetId());
	}

	public void Clear() => _reasons.Clear();
}
