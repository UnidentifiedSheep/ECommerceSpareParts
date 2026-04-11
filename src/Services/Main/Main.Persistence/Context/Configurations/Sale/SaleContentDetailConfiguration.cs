using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Sale;

public class SaleContentDetailConfiguration : IEntityTypeConfiguration<SaleContentDetail>
{
    public void Configure(EntityTypeBuilder<SaleContentDetail> builder)
    {
        builder.HasKey(e => e.Id).HasName("sale_content_details_pk");

        builder.ToTable("sale_content_details");

        builder.HasIndex(e => e.CurrencyId, "sale_content_details_currency_id_index");

        builder.HasIndex(e => e.SaleContentId, "sale_content_details_sale_content_id_index");

        builder.HasIndex(e => e.StorageContentId, "sale_content_details_storage_content_id_index");

        builder.HasIndex(e => e.Storage, "sale_content_details_storage_index");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.BuyPrice).HasColumnName("buy_price");
        builder.Property(e => e.Count).HasColumnName("count");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.PurchaseDatetime).HasColumnName("purchase_datetime");
        builder.Property(e => e.SaleContentId).HasColumnName("sale_content_id");
        builder.Property(e => e.Storage)
            .HasMaxLength(128)
            .HasColumnName("storage");
        
        builder.Property(e => e.StorageContentId).HasColumnName("storage_content_id");

        builder.HasOne<Entities.Currency>()
            .WithMany()
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_content_details_currency_id_fk");

        builder.HasOne<SaleContent>()
            .WithMany(p => p.SaleContentDetails)
            .HasForeignKey(d => d.SaleContentId)
            .HasConstraintName("sale_content_details_sale_content_id_fk");

        builder.HasOne<Storage>()
            .WithMany(p => p.SaleContentDetails)
            .HasForeignKey(d => d.Storage)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_content_details_storages_name_fk");

        builder.HasOne<StorageContent>()
            .WithMany(p => p.SaleContentDetails)
            .HasForeignKey(d => d.StorageContentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("sale_content_details_storage_content_id_fk");
    }
}