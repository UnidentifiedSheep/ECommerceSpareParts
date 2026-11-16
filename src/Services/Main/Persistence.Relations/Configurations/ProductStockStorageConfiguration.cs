using Main.Entities.Product.Stock;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class ProductStockStorageConfiguration : IEntityTypeConfiguration<ProductStock>
{
	public void Configure(EntityTypeBuilder<ProductStock> builder)
		=> builder.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(stock => stock.StorageCode)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("product_stocks_storage_code_fk");
}
