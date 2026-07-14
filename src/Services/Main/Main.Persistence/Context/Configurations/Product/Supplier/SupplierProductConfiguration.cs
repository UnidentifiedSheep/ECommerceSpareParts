using EFCore.ComplexIndexes;
using Main.Entities.Product.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product.Supplier;

public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> builder)
    {
        builder.ToTable("supplier_products", "product_enrichment");

        builder.HasKey(e => e.Id)
            .HasName("supplier_products_pk");
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.Producer)
            .HasColumnName("producer")
            .HasMaxLength(64)
            .IsRequired();
        
        builder.ComplexProperty(
            x => x.Sku,
            b =>
            {
                b.Property(e => e.Value)
                    .HasColumnName("sku")
                    .HasMaxLength(128)
                    .IsRequired();

                b.Property(e => e.NormalizedValue)
                    .HasColumnName("normalized_sku")
                    .HasMaxLength(128)
                    .IsRequired();
            });

        builder.HasIndex(e => e.Producer)
            .HasDatabaseName("supplier_products_producer_idx");

        builder.HasComplexCompositeIndex(
            x => new { x.Sku.NormalizedValue, x.Producer },
            indexName: "supplier_products_normalized_sku_producer_uidx",
            isUnique: true);

        builder.Navigation(e => e.Names)
            .HasField("_names")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
