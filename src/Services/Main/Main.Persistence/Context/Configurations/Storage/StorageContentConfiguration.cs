using Main.Entities;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageContentConfiguration : IEntityTypeConfiguration<StorageContent>
{
    public void Configure(EntityTypeBuilder<StorageContent> builder)
    {
        builder.ToTable("storage_content");
        
        builder.HasKey(e => e.Id).HasName("storage_content_pk");

        builder.HasIndex(e => new { e.ProductId, e.Count }, "storage_content_product_id_count_index");

        builder.HasIndex(e => new { e.ProductId, e.StorageName }, "storage_content_product_id_storage_name_index");

        builder.HasIndex(e => e.BuyPrice, "storage_content_buy_price_index");

        builder.HasIndex(e => e.CurrencyId, "storage_content_currency_id_index");

        builder.HasIndex(e => e.PurchaseDatetime, "storage_content_purchase_datetime_index");

        builder.HasIndex(e => new { e.StorageName, e.ProductId }, "storage_content_storage_name_product_id_index");

        builder.HasIndex(e => e.StorageName, "storage_content_storage_name_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        
        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.BuyPrice)
            .HasColumnName("buy_price");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");
        
        builder.Property(e => e.PurchaseDatetime)
            .HasColumnName("purchase_datetime");
        
        builder.Property(e => e.StorageName)
            .HasMaxLength(128)
            .HasColumnName("storage_name");
        
        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_products_id_fk");

        builder.HasOne<Entities.Currency.Currency>(e => e.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.StorageName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_storages_name_fk");
    }
}