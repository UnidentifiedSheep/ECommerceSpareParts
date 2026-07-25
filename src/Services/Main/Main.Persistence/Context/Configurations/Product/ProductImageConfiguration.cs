using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images", "public");

        builder.HasKey(e => new { e.ProductId, e.StorageKey })
            .HasName("product_images_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.StorageKey)
            .HasColumnName("storage_key")
            .HasMaxLength(255);

        builder.HasOne<Entities.Product.Product>()
            .WithMany(p => p.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_images_product_id_fk");
    }
}