using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductWeightConfiguration : IEntityTypeConfiguration<ProductWeight>
{
    public void Configure(EntityTypeBuilder<ProductWeight> builder)
    {
        builder.ToTable("product_weights");
        
        builder.HasKey(e => e.ProductId)
            .HasName("product_weights_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.Weight)
            .HasColumnName("weight");
        
        builder.Property(e => e.Unit)
            .HasMaxLength(24)
            .HasColumnName("unit");

        builder.HasOne<Entities.Product.Product>()
            .WithOne(e => e.ProductWeight)
            .HasForeignKey<ProductWeight>(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_weight_products_id_fk");
    }
}