using Main.Entities.Product.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Supplier;

public class SupplierProductMappingConfiguration : IEntityTypeConfiguration<SupplierProductMapping>
{
    public void Configure(EntityTypeBuilder<SupplierProductMapping> builder)
    {
        builder.ToTable("supplier_product_mappings", "product_enrichment");

        builder.HasKey(e => e.Id)
            .HasName("supplier_product_mappings_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(e => e.SupplierProductId)
            .HasColumnName("supplier_product_id")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.LastCheckedAt)
            .HasColumnName("last_checked_at");

        builder.HasIndex(e => new { e.ProductId, e.SupplierProductId })
            .HasDatabaseName("supplier_product_mappings_product_supplier_product_uidx")
            .IsUnique();

        builder.HasIndex(e => e.SupplierProductId)
            .HasDatabaseName("supplier_product_mappings_supplier_product_id_idx");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("supplier_product_mappings_status_idx");

        builder.HasOne<Entities.Product.Product>()
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("supplier_product_mappings_product_id_fk");

        builder.HasOne<SupplierProduct>()
            .WithMany()
            .HasForeignKey(e => e.SupplierProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("supplier_product_mappings_supplier_product_id_fk");
    }
}
