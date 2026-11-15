using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations;

public sealed class PurchaseContentRelationsConfiguration : IEntityTypeConfiguration<Entities.Purchase.PurchaseContent>
{
	public void Configure(EntityTypeBuilder<Entities.Purchase.PurchaseContent> builder)
	{
		builder
			.HasOne<Entities.Product.Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_content_products_id_fk");

		builder
			.HasOne<Entities.Storage.StorageContent>()
			.WithOne()
			.HasForeignKey<Entities.Purchase.PurchaseContent>(d => d.StorageContentId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("purchase_content_storage_content_id_fk");
	}
}
