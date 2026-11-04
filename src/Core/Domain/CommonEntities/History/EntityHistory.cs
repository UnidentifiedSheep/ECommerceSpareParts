using System.Linq.Expressions;
using System.Text.Json;
using Domain.CommonEnums;
using Domain.Interfaces;

namespace Domain.CommonEntities.History;

public sealed class EntityHistory : Entity<EntityHistory, Guid>, ILinqEntity<EntityHistory, Guid>
{
	private EntityHistory()
	{
	}

	public Guid Id { get; private set; }
	public string EntityType { get; private set; } = null!;
	public string EntityKey { get; private set; } = null!;
	public string AggregateType { get; private set; } = null!;
	public string AggregateKey { get; private set; } = null!;
	public EntityHistoryOperation Operation { get; private set; }
	public JsonElement Changes { get; private set; }
	public Guid? ActorId { get; private set; }
	public DateTime OccurredAt { get; private set; }
	public Guid ChangeSetId { get; private set; }

	public static EntityHistory Create(
		string entityType,
		string entityKey,
		string aggregateType,
		string aggregateKey,
		EntityHistoryOperation operation,
		JsonElement changes,
		Guid? actorId,
		DateTime occurredAt,
		Guid changeSetId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
		ArgumentException.ThrowIfNullOrWhiteSpace(entityKey);
		ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
		ArgumentException.ThrowIfNullOrWhiteSpace(aggregateKey);
		if (!Enum.IsDefined(operation))
			throw new ArgumentOutOfRangeException(nameof(operation));
		if (changes.ValueKind != JsonValueKind.Object)
			throw new ArgumentException("History changes must be a JSON object.", nameof(changes));
		if (occurredAt.Kind != DateTimeKind.Utc)
			throw new ArgumentException("History timestamp must use UTC.", nameof(occurredAt));
		if (changeSetId == Guid.Empty)
			throw new ArgumentException("History change set ID must not be empty.", nameof(changeSetId));

		return new EntityHistory
		{
			Id = Guid.CreateVersion7(),
			EntityType = entityType,
			EntityKey = entityKey,
			AggregateType = aggregateType,
			AggregateKey = aggregateKey,
			Operation = operation,
			Changes = changes.Clone(),
			ActorId = actorId,
			OccurredAt = occurredAt,
			ChangeSetId = changeSetId
		};
	}

	public override Guid GetId() => Id;
	public static Expression<Func<EntityHistory, Guid>> GetKeySelector() => entry => entry.Id;
	public static Expression<Func<EntityHistory, bool>> GetEqualityExpression(Guid key) => entry => entry.Id == key;
}
