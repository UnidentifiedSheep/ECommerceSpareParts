using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Sale;

public class SaleContentConfiguration : IEntityTypeConfiguration<SaleContent>
{
    public void Configure(EntityTypeBuilder<SaleContent> builder)
    {
        builder.HasKey(e => e.Id).HasName("sale_contents_pk");

        builder.ToTable("sale_contents");

        builder.HasIndex(e => e.ProductId, "sale_contents_product_id_index");

        builder.HasIndex(e => e.SaleId, "sale_contents_sale_id_index");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(e => e.ProductId).HasColumnName("product_id");
        builder.Property(e => e.Count).HasColumnName("count");
        builder.Property(e => e.Discount).HasColumnName("discount");
        builder.Property(e => e.Price).HasColumnName("price");
        builder.Property(e => e.SaleId)
            .HasMaxLength(128)
            .HasColumnName("sale_id");

        builder.HasOne(d => d.Sale).WithMany(p => p.SaleContents)
            .HasForeignKey(d => d.SaleId)
            .HasConstraintName("sale_contents_sales_fact_id_fk");
    }
}