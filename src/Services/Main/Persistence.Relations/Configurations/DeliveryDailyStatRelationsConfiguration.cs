using Main.Entities.Logistics.Statistics;
using Main.Entities.Product.Product;
using Main.Entities.Storage.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Relations.Configurations;

public sealed class DeliveryDailyStatRelationsConfiguration : IEntityTypeConfiguration<DeliveryDailyStat>
{
	public void Configure(EntityTypeBuilder<DeliveryDailyStat> builder)
	{
		builder
			.HasOne<Product>()
			.WithMany()
			.HasForeignKey(x => x.ProductId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("delivery_daily_stat_product_id_fk");

		builder
			.HasOne<Storage>()
			.WithMany()
			.HasForeignKey(x => x.StorageCode)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("delivery_daily_stat_storage_code_fk");
	}
}
