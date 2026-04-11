using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Purchase;

public class PurchaseContentConfiguration : IEntityTypeConfiguration<PurchaseContent>
{
    public void Configure(EntityTypeBuilder<PurchaseContent> builder)
    {
        builder.ToTable("purchase_content");
        
        builder.HasKey(e => e.Id).HasName("purchase_content_pk");

        builder.HasIndex(e => e.ProductId, "purchase_content_product_id_index");

        builder.HasIndex(e => e.Comment, "purchase_content_comment_index")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => e.PurchaseId, "purchase_content_purchase_id_index");

        builder.HasIndex(e => e.StorageContentId, "purchase_content_storage_content_id_uindex")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("id");
        
        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");
        
        builder.Property(e => e.Comment)
            .HasMaxLength(256)
            .HasColumnName("comment");
        
        builder.Property(e => e.Count)
            .HasColumnName("count");
        
        builder.Property(e => e.Price)
            .HasColumnName("price");
        
        builder.Property(e => e.PurchaseId)
            .HasColumnName("purchase_id");
        
        builder.Property(e => e.StorageContentId)
            .HasColumnName("storage_content_id");
        
        builder.Property(e => e.TotalSum)
            .HasColumnName("total_sum");

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("purchase_content_products_id_fk");

        builder.HasOne<Entities.Purchase>()
            .WithMany(p => p.PurchaseContents)
            .HasForeignKey(d => d.PurchaseId)
            .HasConstraintName("purchase_content_purchase_id_fk");

        builder.HasOne<StorageContent>()
            .WithOne()
            .HasForeignKey<PurchaseContent>(d => d.StorageContentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("purchase_content_storage_content_id_fk");
    }
}