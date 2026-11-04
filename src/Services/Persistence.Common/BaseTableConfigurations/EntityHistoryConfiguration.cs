using System.Text.Json;
using Domain.CommonEntities.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public sealed class EntityHistoryConfiguration : IEntityTypeConfiguration<EntityHistory>
{
	public void Configure(EntityTypeBuilder<EntityHistory> builder)
	{
		builder.ToTable("entity_history", "audit");
		builder.HasKey(history => history.Id).HasName("entity_history_pk");
		builder.Property(history => history.Id).HasColumnName("id").ValueGeneratedNever();
		builder.Property(history => history.EntityType).HasColumnName("entity_type").IsRequired();
		builder.Property(history => history.EntityKey).HasColumnName("entity_key").IsRequired();
		builder.Property(history => history.AggregateType).HasColumnName("aggregate_type").IsRequired();
		builder.Property(history => history.AggregateKey).HasColumnName("aggregate_key").IsRequired();
		builder.Property(history => history.Operation).HasColumnName("operation").HasConversion<string>();
		builder.Property(history => history.Changes).HasColumnName("changes").HasColumnType("jsonb").IsRequired();
		builder.Property(history => history.Reasons).HasColumnName("reasons").HasColumnType("jsonb")
			.IsRequired()
			.HasConversion(
				reasons => JsonSerializer.Serialize(reasons, (JsonSerializerOptions?)null),
				json => JsonSerializer.Deserialize<EntityHistoryReason[]>(json, (JsonSerializerOptions?)null)!)
			.Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<EntityHistoryReason>>(
				(left, right) => left!.SequenceEqual(right!),
				reasons => reasons.Aggregate(0, (hash, reason) => HashCode.Combine(hash, reason.GetHashCode())),
				reasons => reasons.ToArray()));
		builder.Property(history => history.ActorId).HasColumnName("actor_id");
		builder.Property(history => history.OccurredAt).HasColumnName("occurred_at");
		builder.Property(history => history.ChangeSetId).HasColumnName("change_set_id");

		builder.HasIndex(history => new { history.EntityType, history.EntityKey, history.OccurredAt, history.Id },
			"entity_history_entity_type_entity_key_occurred_at_id_idx").IsDescending(false, false, true, true);
		builder.HasIndex(history => new { history.AggregateType, history.AggregateKey, history.OccurredAt, history.Id },
			"entity_history_aggregate_type_aggregate_key_occurred_at_id_idx").IsDescending(false, false, true, true);
		builder.HasIndex(history => history.ChangeSetId, "entity_history_change_set_id_idx");
	}
}
