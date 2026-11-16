using Main.Entities.Product.Product;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class StorageContentRelationsConfiguration : IEntityTypeConfiguration<StorageContent>
{
	public void Configure(EntityTypeBuilder<StorageContent> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(d => d.ProductId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("storage_content_products_id_fk");

		builder
			.HasOne<Main.Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.CurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("storage_content_currency_id_fk");

		builder
			.HasOne<Main.Entities.Currency.Currency>()
			.WithMany()
			.HasForeignKey(d => d.BaseCurrencyId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("storage_content_base_currency_id_fk");
	}
}
