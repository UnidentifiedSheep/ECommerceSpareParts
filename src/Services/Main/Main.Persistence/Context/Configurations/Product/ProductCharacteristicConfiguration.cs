using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductCharacteristicConfiguration : IEntityTypeConfiguration<ProductCharacteristic>
{
    public void Configure(EntityTypeBuilder<ProductCharacteristic> builder)
    {
        builder.HasKey(e => e.ProductId).HasName("product_characteristics_pk");

        builder.ToTable("product_characteristics");

        builder.HasIndex(e => e.Value, "product_characteristics_value_index");

        builder.HasIndex(e => e.ProductId, "product_id__index");

        builder.Property(e => e.ProductId).HasColumnName("product_id");
        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name");
        builder.Property(e => e.Value)
            .HasMaxLength(128)
            .HasColumnName("value");

        builder.HasOne(d => d.Product)
            .WithMany(p => p.ArticleCharacteristics)
            .HasForeignKey(d => d.ProductId)
            .HasConstraintName("product_id_fk");
    }
}