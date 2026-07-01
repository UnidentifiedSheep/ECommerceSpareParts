using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Purchase;

public class PurchaseContentConfiguration : IEntityTypeConfiguration<PurchaseContent>
{
    public void Configure(EntityTypeBuilder<PurchaseContent> builder)
    {
        builder.HasKey(e => e.Id).HasName("purchase_contents_pk");

        builder.ToTable("purchase_contents");

        builder.HasIndex(e => e.ProductId, "purchase_contents_product_id_index");

        builder.HasIndex(e => e.PurchaseId, "purchase_contents_purchase_id_index");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(e => e.ProductId).HasColumnName("product_id");
        builder.Property(e => e.Count).HasColumnName("count");
        builder.Property(e => e.Price).HasColumnName("price");
        builder.Property(e => e.PurchaseId).HasColumnName("purchase_id");

        builder.HasOne(d => d.Purchase)
            .WithMany(p => p.PurchaseContents)
            .HasForeignKey(d => d.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("purchase_contents_purchases_fact_id_fk");
    }
}