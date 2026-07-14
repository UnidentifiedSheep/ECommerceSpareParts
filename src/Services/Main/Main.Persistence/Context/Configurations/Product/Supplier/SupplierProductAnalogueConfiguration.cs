using Main.Entities.Product.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Supplier;

public class SupplierProductAnalogueConfiguration : IEntityTypeConfiguration<SupplierProductAnalogue>
{
    public void Configure(EntityTypeBuilder<SupplierProductAnalogue> builder)
    {
        builder.ToTable("supplier_product_analogues", "product_enrichment");

        builder.HasKey(e => new { e.SupplierProductId, e.SupplierAnalogueProductId })
            .HasName("supplier_product_analogues_pk");

        builder.Property(e => e.SupplierProductId)
            .HasColumnName("supplier_product_id")
            .IsRequired();

        builder.Property(e => e.SupplierAnalogueProductId)
            .HasColumnName("supplier_analogue_product_id")
            .IsRequired();

        builder.HasOne<SupplierProduct>()
            .WithMany()
            .HasForeignKey(e => e.SupplierProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("supplier_product_analogues_supplier_product_id_fk");

        builder.HasOne<SupplierProduct>()
            .WithMany()
            .HasForeignKey(e => e.SupplierAnalogueProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("supplier_product_analogues_supplier_analogue_product_id_fk");

        builder.HasIndex(e => e.SupplierAnalogueProductId)
            .HasDatabaseName("supplier_product_analogues_supplier_analogue_product_id_idx");
    }
}
