using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Contexts.Configurations;

public class PriceOfferDailyStatConfiguration : IEntityTypeConfiguration<PriceOfferDailyStat>
{
	public void Configure(EntityTypeBuilder<PriceOfferDailyStat> builder)
	{
		builder.ToTable("price_offer_daily_stats", "public");

		builder
			.HasKey(e => new
			{
				e.ProductId,
				e.Source,
				e.ForStorage,
				e.Day,
				e.CurrencyId,
				e.PurchasePrice
			})
			.HasName("price_offer_daily_stat_pk");

		builder.HasIndex(e => e.CurrencyId, "price_offer_daily_stat_currency_id_index");

		builder.HasIndex(e => e.ForStorage, "price_offer_daily_stat_for_storage_index");

		builder.Property(e => e.Day).HasColumnName("day");

		builder.Property(e => e.ProductId).HasColumnName("product_id");

		builder.Property(e => e.Source).HasMaxLength(64).HasColumnName("source");

		builder.Property(e => e.ForStorage).HasMaxLength(128).HasColumnName("for_storage");

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.PurchasePrice).HasColumnName("price");

		builder.Property(e => e.OffersCount).HasColumnName("offers_count");

		builder.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at");

		builder.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
	}
}
