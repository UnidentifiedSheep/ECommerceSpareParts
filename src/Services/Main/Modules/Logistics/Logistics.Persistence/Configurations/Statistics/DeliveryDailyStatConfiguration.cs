using Main.Entities.Logistics.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Logistics.Persistence.Configurations.Statistics;

public sealed class DeliveryDailyStatConfiguration : IEntityTypeConfiguration<DeliveryDailyStat>
{
	public void Configure(EntityTypeBuilder<DeliveryDailyStat> builder)
	{
		builder.ToTable("delivery_daily_stats", "logistics");

		builder
			.HasKey(x => new
			{
				x.ProductId,
				x.Supplier,
				x.StorageCode,
				x.Day,
				x.DeliveryDays
			})
			.HasName("delivery_daily_stat_pk");

		builder.HasIndex(x => x.StorageCode, "delivery_daily_stat_storage_code_index");

		builder.Property(x => x.Day).HasColumnName("day");

		builder.Property(x => x.Supplier).HasColumnName("supplier").HasMaxLength(32);

		builder.Property(x => x.StorageCode).HasColumnName("storage_code").HasMaxLength(128);

		builder.Property(x => x.ProductId).HasColumnName("product_id");

		builder.Property(x => x.DeliveryDays).HasColumnName("delivery_days");

		builder.Property(x => x.OffersCount).HasColumnName("offers_count");

		builder.Property(x => x.MaxProbability).HasColumnName("max_probability");

		builder.Property(x => x.MinGuaranteedDays).HasColumnName("min_guaranteed_days");

		builder.Property(x => x.FirstSeenAt).HasColumnName("first_seen_at");

		builder.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
	}
}
