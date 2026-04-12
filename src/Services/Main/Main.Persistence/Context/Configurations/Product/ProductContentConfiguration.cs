using Main.Entities;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductContentConfiguration : IEntityTypeConfiguration<ProductContent>
{
    public void Configure(EntityTypeBuilder<ProductContent> builder)
    {
        builder.ToTable("product_contents");
        
        builder.HasKey(e => new { e.ParentProductId, e.ChildProductId })
            .HasName("product_contents_pk");
        
        builder.HasIndex(e => e.ChildProductId)
            .HasDatabaseName("product_contents_child_id_idx");
        
        builder.Property(e => e.ParentProductId)
            .HasColumnName("parent_product_id");
        
        builder.Property(e => e.ChildProductId)
            .HasColumnName("child_product_id");

        builder.Property(e => e.Quantity)
            .HasColumnName("quantity");
        
        builder.HasOne(x => x.ParentProduct)
            .WithMany(p => p.Contents)
            .HasForeignKey(x => x.ParentProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("product_contents_parent_fk");

        builder.HasOne(x => x.ChildProduct)
            .WithMany()
            .HasForeignKey(x => x.ChildProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("product_contents_child_fk");
    }
}