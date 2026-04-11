using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageMovementConfiguration : IEntityTypeConfiguration<StorageMovement>
{
    public void Configure(EntityTypeBuilder<StorageMovement> builder)
    {
        builder.HasKey(e => e.Id).HasName("storage_movement_pk");

        builder.ToTable("storage_movement");

        builder.HasIndex(e => e.ProductId, "storage_movement_product_id_index");

        builder.HasIndex(e => e.CreatedAt, "storage_movement_created_at_index");

        builder.HasIndex(e => e.StorageName, "storage_movement_storage_name_index");

        builder.HasIndex(e => e.WhoMoved, "storage_movement_who_moved_index");
        
        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.ActionType)
            .HasMaxLength(24)
            .HasColumnName("action_type");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnName("created_at")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.Price)
            .HasColumnName("price");
        
        builder.Property(e => e.StorageName)
            .HasMaxLength(128)
            .HasColumnName("storage_name");
        
        builder.Property(e => e.WhoMoved)
            .HasColumnName("who_moved");
        
        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_movement_products_id_fk");

        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_movement_currency_id_fk");

        builder.HasOne<Entities.Storage>()
            .WithMany()
            .HasForeignKey(d => d.StorageName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_movement_storages_name_fk");

        builder.HasOne<Entities.User>()
            .WithMany()
            .HasForeignKey(d => d.WhoMoved)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_movement_users_id_fk");
    }
}