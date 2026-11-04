using System.Linq.Expressions;
using System.Text.Json;
using Domain.CommonEnums;
using Domain.Interfaces;

namespace Domain.CommonEntities.History;

public sealed class EntityHistory : Entity<EntityHistory, Guid>, ILinqEntity<EntityHistory, Guid>
{
	private EntityHistory() { }

	public Guid Id { get; private set; }
	public string EntityType { get; private set; } = null!;
	public string EntityKey { get; private set; } = null!;
	public string AggregateType { get; private set; } = null!;
	public string AggregateKey { get; private set; } = null!;
	public EntityHistoryOperation Operation { get; private set; }
	public JsonElement Changes { get; private set; }
	public IReadOnlyList<EntityHistoryReason> Reasons { get; private set; } = [];
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
		Guid changeSetId,
		DateTime occurredAt,
		Guid? actorId = null,
		IEnumerable<EntityHistoryReason>? reasons = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
		ArgumentException.ThrowIfNullOrWhiteSpace(entityKey);
		ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
		ArgumentException.ThrowIfNullOrWhiteSpace(aggregateKey);
		if (!Enum.IsDefined(operation))
			throw new ArgumentOutOfRangeException(nameof(operation));
		if (changes.ValueKind != JsonValueKind.Object)
			throw new ArgumentException("Changes must be a JSON object.", nameof(changes));
		if (changeSetId == Guid.Empty)
			throw new ArgumentException("Change set ID must not be empty.", nameof(changeSetId));
		if (occurredAt.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Occurrence time must be UTC.", nameof(occurredAt));

		var uniqueReasons = reasons?.Distinct().ToArray() ?? [];
		foreach (var reason in uniqueReasons)
		{
			ArgumentNullException.ThrowIfNull(reason);
			ArgumentException.ThrowIfNullOrWhiteSpace(reason.Code);
			if (reason.SourceType is not null || reason.SourceKey is not null)
			{
				ArgumentException.ThrowIfNullOrWhiteSpace(reason.SourceType);
				ArgumentException.ThrowIfNullOrWhiteSpace(reason.SourceKey);
			}
		}

		return new EntityHistory
		{
			Id = Guid.CreateVersion7(),
			EntityType = entityType,
			EntityKey = entityKey,
			AggregateType = aggregateType,
			AggregateKey = aggregateKey,
			Operation = operation,
			Changes = changes.Clone(),
			Reasons = Array.AsReadOnly(uniqueReasons),
			ChangeSetId = changeSetId,
			OccurredAt = occurredAt,
			ActorId = actorId
		};
	}

	public override Guid GetId() => Id;
	public static Expression<Func<EntityHistory, Guid>> GetKeySelector() => history => history.Id;
	public static Expression<Func<EntityHistory, bool>> GetEqualityExpression(Guid key) => history => history.Id == key;
}
