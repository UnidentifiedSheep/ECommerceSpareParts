using Main.Entities;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductEanConfiguration : IEntityTypeConfiguration<ProductEan>
{
    public void Configure(EntityTypeBuilder<ProductEan> builder)
    {
        builder.ToTable("product_eans");
        
        builder.HasKey(e => new { e.ProductId, e.Ean })
            .HasName("product_eans_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.Ean)
            .HasColumnName("ean")
            .HasMaxLength(30);
        
        builder.HasOne<Entities.Product.Product>()
            .WithMany(p => p.ProductEans)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_eans_product_id_fk");
    }
}