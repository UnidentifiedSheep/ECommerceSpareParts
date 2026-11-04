using System.Text.Json;
using Domain.CommonEntities.History;
using Domain.CommonEnums;

namespace Tests.DataBuilders;

internal sealed class EntityHistoryBuilder
{
	private string _entityType = "Product";
	private string _entityKey = "42";
	private string? _aggregateType;
	private string? _aggregateKey;
	private Guid _changeSetId = Guid.NewGuid();
	private Guid? _actorId;

	public EntityHistoryBuilder WithEntity(string type, string key)
	{
		_entityType = type;
		_entityKey = key;
		return this;
	}

	public EntityHistoryBuilder WithAggregate(string type, string key)
	{
		_aggregateType = type;
		_aggregateKey = key;
		return this;
	}

	public EntityHistoryBuilder WithChangeSetId(Guid changeSetId)
	{
		_changeSetId = changeSetId;
		return this;
	}

	public EntityHistoryBuilder WithActorId(Guid actorId)
	{
		_actorId = actorId;
		return this;
	}

	public EntityHistory Build() => EntityHistory.Create(
		_entityType, _entityKey, _aggregateType ?? _entityType, _aggregateKey ?? _entityKey,
		EntityHistoryOperation.Updated,
		JsonSerializer.SerializeToElement(new { Count = new { Old = 10, New = 7 } }),
		_actorId, DateTime.UtcNow, _changeSetId);
}
