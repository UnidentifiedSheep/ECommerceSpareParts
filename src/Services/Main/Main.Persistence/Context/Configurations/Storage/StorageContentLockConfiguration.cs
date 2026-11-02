using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public sealed class StorageContentLockConfiguration : IEntityTypeConfiguration<StorageContentLock>
{
	public void Configure(EntityTypeBuilder<StorageContentLock> builder)
	{
		builder.ToTable("storage_content_locks", "public", table =>
		{
			table.HasCheckConstraint("storage_content_locks_quantity_positive_ck", "quantity > 0");
		});

		builder.HasKey(item => item.Id)
			.HasName("storage_content_locks_pk");
		builder.Property(item => item.Id)
			.HasColumnName("id")
			.ValueGeneratedNever();

		builder.Property(item => item.StorageContentId)
			.HasColumnName("storage_content_id");

		builder.Property(item => item.Quantity)
			.HasColumnName("quantity");

		builder.Property(item => item.State)
			.HasColumnName("state")
			.HasConversion<string>()
			.HasMaxLength(16);

		builder.Property(item => item.ExpiresAt)
			.HasColumnName("expires_at");

		builder.Property(item => item.ClosedAt)
			.HasColumnName("closed_at");

		builder.Property(item => item.RowVersion)
			.HasColumnName("xmin")
			.IsRowVersion();

		builder.HasIndex(item => item.StorageContentId, "storage_content_locks_storage_content_id_idx");

		builder.HasIndex(item => item.StorageContentId, "storage_content_locks_active_content_idx")
			.HasFilter($"state = '{nameof(StorageContentLockState.Active)}'")
			.IncludeProperties(item => new { item.Quantity, item.ExpiresAt });

		builder.HasIndex(item => new { item.ExpiresAt, item.Id }, "storage_content_locks_active_expiration_idx")
			.HasFilter($"state = '{nameof(StorageContentLockState.Active)}' AND expires_at IS NOT NULL");

		builder.HasOne(item => item.StorageContent)
			.WithMany()
			.HasForeignKey(item => item.StorageContentId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("storage_content_locks_storage_content_id_fk");
	}
}
