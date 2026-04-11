using Main.Entities;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        
        builder.HasKey(e => new { e.ProductId, e.Path})
            .HasName("product_images_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.Path)
            .HasColumnName("path")
            .HasMaxLength(255);
        
        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(512);
        
        builder.HasOne<Entities.Product.Product>()
            .WithMany(p => p.ProductImages)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_images_product_id_fk");
    }
}