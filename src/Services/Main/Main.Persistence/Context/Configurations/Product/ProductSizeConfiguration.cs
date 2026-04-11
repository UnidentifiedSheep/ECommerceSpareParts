using Main.Entities;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
{
    public void Configure(EntityTypeBuilder<ProductSize> builder)
    {
        builder.ToTable("product_sizes");
        
        builder.HasKey(e => e.ProductId)
            .HasName("product_sizes_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Height).HasColumnName("height");
        builder.Property(e => e.Length).HasColumnName("length");
        builder.Property(e => e.Unit)
            .HasMaxLength(24)
            .HasColumnName("unit");
        builder.Property(e => e.VolumeM3).HasColumnName("volume_m3");
        builder.Property(e => e.Width).HasColumnName("width");

        builder.HasOne<Entities.Product.Product>()
            .WithOne(e => e.ProductSize)
            .HasForeignKey<ProductSize>(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_sizes_products_id_fk");
    }
}