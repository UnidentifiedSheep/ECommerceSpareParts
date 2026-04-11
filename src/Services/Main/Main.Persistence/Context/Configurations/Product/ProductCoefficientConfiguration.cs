using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Product;

public class ProductCoefficientConfiguration : IEntityTypeConfiguration<ProductCoefficient>
{
    public void Configure(EntityTypeBuilder<ProductCoefficient> builder)
    {
        builder.ToTable("product_coefficients");
        
        builder.HasKey(e => new { e.ProductId, e.CoefficientName })
            .HasName("product_coefficients_pk");

        builder.Property(e => e.CoefficientName)
            .HasColumnName("coefficient_name")
            .HasMaxLength(50);

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.ValidTill)
            .HasColumnName("valid_till");
        
        builder.HasOne(d => d.Coefficient)
            .WithMany(p => p.ArticleCoefficients)
            .HasForeignKey(d => d.CoefficientName)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("article_coefficients_coefficients_name_fk");
    }
}