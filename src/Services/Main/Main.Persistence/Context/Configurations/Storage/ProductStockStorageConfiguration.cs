using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Persistence.Context.Configurations.Storage;

// The stock row itself belongs to the Product module; the link to the storage table is owned by Main.
public sealed class ProductStockStorageConfiguration : IEntityTypeConfiguration<ProductStock>
{
	public void Configure(EntityTypeBuilder<ProductStock> builder)
		=> builder.HasOne<Entities.Storage.Storage>()
			.WithMany()
			.HasForeignKey(stock => stock.StorageCode)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_stocks_storage_code_fk");
}
