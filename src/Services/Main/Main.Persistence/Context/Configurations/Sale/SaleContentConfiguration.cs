using Main.Entities;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Sale;

public class SaleContentConfiguration : IEntityTypeConfiguration<SaleContent>
{
    public void Configure(EntityTypeBuilder<SaleContent> builder)
    {
        builder.ToTable("sale_content");
        
        builder.HasKey(e => e.Id).HasName("sale_content_pk");

        builder.HasIndex(e => e.ProductId, "sale_content_product_id_index");

        builder.HasIndex(e => e.Comment, "sale_content_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.SaleId, "sale_content_sale_id_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.Property(e => e.Discount)
            .HasColumnName("discount");
        
        builder.Property(e => e.Price)
            .HasColumnName("price");
        
        builder.Property(e => e.SaleId)
            .HasColumnName("sale_id");
        
        builder.Property(e => e.TotalSum)
            .HasColumnName("total_sum");

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("sale_content_products_id_fk");

        builder.HasOne<Entities.Sale.Sale>()
            .WithMany(p => p.Contents)
            .HasForeignKey(d => d.SaleId)
            .HasConstraintName("sale_content_sale_id_fk");
        
        builder.Navigation(e => e.Details)
            .HasField("_details")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}