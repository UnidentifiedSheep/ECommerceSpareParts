using Main.Entities.Purchase.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Purchase.Persistence.Configurations;

public class PurchaseContentConfiguration : IEntityTypeConfiguration<PurchaseContent>
{
	public void Configure(EntityTypeBuilder<PurchaseContent> builder)
	{
		builder.ToTable("purchase_content", "public");

		builder.HasKey(e => e.Id).HasName("purchase_content_pk");

		builder.HasIndex(e => e.ProductId, "purchase_content_product_id_index");

		builder
			.HasIndex(e => e.Comment, "purchase_content_comment_index")
			.HasMethod("gin")
			.HasOperators("gin_trgm_ops");

		builder.HasIndex(e => e.PurchaseId, "purchase_content_purchase_id_index");

		builder.HasIndex(e => e.StorageContentId, "purchase_content_storage_content_id_uindex").IsUnique();

		builder.Property(e => e.Id).HasColumnName("id");

		builder.Property(e => e.ProductId).HasColumnName("product_id");

		builder.Property(e => e.Comment).HasMaxLength(256).HasColumnName("comment");

		builder.Property(e => e.Count).HasColumnName("count");

		builder.Property(e => e.Price).HasColumnName("price");

		builder.Property(e => e.PurchaseId).HasColumnName("purchase_id");

		builder.Property(e => e.StorageContentId).HasColumnName("storage_content_id");

		builder.Property(e => e.TotalSum).HasColumnName("total_sum");

		builder
			.HasOne<Entities.Purchase.Purchase.Purchase>()
			.WithMany(p => p.Contents)
			.HasForeignKey(d => d.PurchaseId)
			.HasConstraintName("purchase_content_purchase_id_fk");
	}
}
