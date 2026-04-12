using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductCrossConfiguration : IEntityTypeConfiguration<ProductCross>
{
    public void Configure(EntityTypeBuilder<ProductCross> builder)
    {
        builder.ToTable("product_crosses");

        builder.HasKey(x => new { x.LeftProductId, x.RightProductId })
            .HasName("product_crosses_pk");

        builder
            .HasOne(x => x.LeftProduct)
            .WithMany()
            .HasForeignKey(x => x.LeftProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.RightProduct)
            .WithMany()
            .HasForeignKey(x => x.RightProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LeftProductId)
            .HasColumnName("left_product_id")
            .IsRequired();
        builder.Property(x => x.RightProductId)
            .HasColumnName("right_product_id")
            .IsRequired();

        builder.HasIndex(e => e.RightProductId)
            .HasDatabaseName("product_crosses_right_id_idx");
    }
}