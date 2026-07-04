using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

public class StorageContentConfiguration : IEntityTypeConfiguration<StorageContent>
{
    public void Configure(EntityTypeBuilder<StorageContent> builder)
    {
        builder.ToTable("storage_content", "public");

        builder.HasKey(e => e.Id).HasName("storage_content_pk");

        builder.HasIndex(
            e => new { e.ProductId, e.StorageName },
            "storage_content_product_id_storage_name_index");

        builder.HasIndex(
                e => new { e.ProductId, e.StorageName },
                "storage_content_product_storage_positive_count_idx")
            .HasFilter("(count > 0)")
            .IncludeProperties(e => e.Count);

        builder.HasIndex(e => e.CurrencyId, "storage_content_currency_id_index");

        builder.HasIndex(
            e => new { e.StorageName, e.ProductId },
            "storage_content_storage_name_product_id_index");

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

        builder.Property(e => e.BaseCurrencyId)
            .HasColumnName("base_currency_id");

        builder.Property(e => e.BuyPriceInBaseCurrency)
            .HasColumnName("buy_price_in_base_currency");

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

        builder.HasOne<Entities.Currency.Currency>()
            .WithMany()
            .HasForeignKey(d => d.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_base_currency_id_fk");

        builder.HasOne<Entities.Storage.Storage>()
            .WithMany()
            .HasForeignKey(d => d.StorageName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("storage_content_storages_name_fk");
    }
}