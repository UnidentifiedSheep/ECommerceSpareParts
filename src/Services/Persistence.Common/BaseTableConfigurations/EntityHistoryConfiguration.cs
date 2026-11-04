using Domain.CommonEntities.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public sealed class EntityHistoryConfiguration : IEntityTypeConfiguration<EntityHistory>
{
	public void Configure(EntityTypeBuilder<EntityHistory> builder)
	{
		builder.ToTable("entity_history", "audit");
		builder.HasKey(entry => entry.Id)
			.HasName("entity_history_pk");

		builder.Property(entry => entry.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(entry => entry.EntityType)
			.HasColumnName("entity_type")
			.IsRequired();

		builder.Property(entry => entry.EntityKey)
			.HasColumnName("entity_key")
			.IsRequired();

		builder.Property(entry => entry.AggregateType)
			.HasColumnName("aggregate_type")
			.IsRequired();

		builder.Property(entry => entry.AggregateKey)
			.HasColumnName("aggregate_key")
			.IsRequired();

		builder.Property(entry => entry.Operation)
			.HasColumnName("operation")
			.HasConversion<string>()
			.IsRequired();

		builder.Property(entry => entry.Changes)
			.HasColumnName("changes")
			.HasColumnType("jsonb")
			.IsRequired();

		builder.Property(entry => entry.ActorId)
			.HasColumnName("actor_id");

		builder.Property(entry => entry.OccurredAt)
			.HasColumnName("occurred_at");

		builder.Property(entry => entry.ChangeSetId)
			.HasColumnName("change_set_id");

		builder.HasIndex(entry => new { entry.EntityType, entry.EntityKey, entry.OccurredAt, entry.Id },
			"entity_history_entity_type_entity_key_occurred_at_id_idx").IsDescending(false, false, true, true);

		builder.HasIndex(entry => new { entry.AggregateType, entry.AggregateKey, entry.OccurredAt, entry.Id },
			"entity_history_aggregate_type_aggregate_key_occurred_at_id_idx").IsDescending(false, false, true, true);

		builder.HasIndex(entry => entry.ChangeSetId, "entity_history_change_set_id_idx");
	}
}
