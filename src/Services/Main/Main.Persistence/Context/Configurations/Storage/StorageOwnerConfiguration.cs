using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageOwnerConfiguration : IEntityTypeConfiguration<StorageOwner>
{
    public void Configure(EntityTypeBuilder<StorageOwner> builder)
    {
        builder.ToTable("storage_owners", "public");

        builder.HasKey(e => new { e.StorageCode, e.UserId }).HasName("storage_owners_pk");

        builder.HasIndex(e => e.UserId, "storage_owners_owner_id_index");

        builder.Property(e => e.StorageCode)
            .HasMaxLength(128)
            .HasColumnName("storage_name");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("storage_owners_users_id_fk");

        builder.HasOne(d => d.Storage)
            .WithMany(d => d.Owners)
            .HasForeignKey(d => d.StorageCode)
            .HasConstraintName("storage_owners_storages_name_fk");
    }
}