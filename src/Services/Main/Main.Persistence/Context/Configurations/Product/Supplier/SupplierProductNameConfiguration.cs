using Main.Entities.Product.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Supplier;

public class SupplierProductNameConfiguration 
    : IEntityTypeConfiguration<SupplierProductName>
{
    public void Configure(EntityTypeBuilder<SupplierProductName> builder)
    {
        builder.ToTable("supplier_product_names", "product_enrichment");

        builder.HasKey(e => e.Id)
            .HasName("supplier_product_names_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.SupplierProductId)
            .HasColumnName("supplier_product_id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Supplier)
            .HasColumnName("supplier")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(e => new { e.SupplierProductId, e.Name })
            .HasDatabaseName("supplier_product_names_product_supplier_name_uidx")
            .IsUnique();

        builder.HasOne(e => e.SupplierProduct)
            .WithMany(e => e.Names)
            .HasForeignKey(e => e.SupplierProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("supplier_product_names_supplier_product_id_fk");
    }
}
