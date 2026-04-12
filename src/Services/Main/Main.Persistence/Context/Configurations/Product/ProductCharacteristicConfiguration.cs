using Main.Entities;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductCharacteristicConfiguration : IEntityTypeConfiguration<ProductCharacteristic>
{
    public void Configure(EntityTypeBuilder<ProductCharacteristic> builder)
    {
        builder.ToTable("product_characteristics");
        
        builder.HasKey(e => new { e.ProductId, e.Name})
            .HasName("product_characteristics_pk");

        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("product_characteristics_id_index");
        
        builder.HasIndex(e => new { e.Name, e.Value })
            .HasDatabaseName("product_characteristics_name_value_index");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .HasColumnName("name");
        
        builder.Property(e => e.Value)
            .HasMaxLength(128)
            .HasColumnName("value");

        builder.HasOne<Entities.Product.Product>()
            .WithMany(p => p.Characteristics)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_characteristics_product_id_fk");
    }
}