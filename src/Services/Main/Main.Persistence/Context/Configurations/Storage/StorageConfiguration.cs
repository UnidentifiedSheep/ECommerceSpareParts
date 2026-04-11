using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageConfiguration : IEntityTypeConfiguration<Entities.Storage.Storage>
{
    public void Configure(EntityTypeBuilder<Entities.Storage.Storage> builder)
    {
        builder.HasKey(e => e.Name).HasName("storages_pk");

        builder.ToTable("storages");

        builder.HasIndex(e => e.Description, "storages_description_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Location, "storages_location_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.Type, "storages_type_index");

        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name");
        builder.Property(e => e.Description)
            .HasMaxLength(256)
            .HasColumnName("description");
        builder.Property(e => e.Location)
            .HasMaxLength(256)
            .HasColumnName("location");
        builder.Property(e => e.Type)
            .HasMaxLength(24)
            .HasColumnName("type");
    }
}