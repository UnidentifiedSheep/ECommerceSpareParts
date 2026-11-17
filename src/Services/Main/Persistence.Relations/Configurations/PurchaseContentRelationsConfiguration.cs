using Main.Entities.Product.Product;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class PurchaseContentRelationsConfiguration : IEntityTypeConfiguration<PurchaseContent>
{
	public void Configure(EntityTypeBuilder<PurchaseContent> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("purchase_content_products_id_fk");

		builder
			.HasOne<StorageContent>()
			.WithOne()
			.HasForeignKey<PurchaseContent>(d => d.StorageContentId)
			.OnDelete(DeleteBehavior.SetNull)
			.HasConstraintName("purchase_content_storage_content_id_fk");
	}
}
