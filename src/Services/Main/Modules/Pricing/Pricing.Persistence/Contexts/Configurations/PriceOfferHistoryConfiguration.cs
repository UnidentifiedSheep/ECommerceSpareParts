using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Contexts.Configurations;

public class PriceOfferHistoryConfiguration : IEntityTypeConfiguration<PriceOfferHistory>
{
	public void Configure(EntityTypeBuilder<PriceOfferHistory> builder)
	{
		builder.ToTable("price_offer_histories", "public");

		builder.HasKey(e => e.Id).HasName("price_offer_history_pk");

		builder.HasIndex(
				e => new
				{
					e.ProductId,
					e.Source,
					e.ForStorage
				},
				"price_offer_history_open_index")
			.HasFilter("is_open = true");

		builder.HasIndex(
			e => new
			{
				e.ProductId,
				e.ForStorage,
				e.FirstSeenAt
			},
			"price_offer_history_product_index");

		builder.HasIndex(e => e.CurrencyId, "price_offer_history_currency_id_index");

		builder.HasIndex(e => e.ForStorage, "price_offer_history_for_storage_index");

		builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

		builder.Property(e => e.ProductId).HasColumnName("product_id");

		builder.Property(e => e.Source).HasMaxLength(64).HasColumnName("source");

		builder.Property(e => e.ForStorage).HasMaxLength(128).HasColumnName("for_storage");

		builder.Property(e => e.CurrencyId).HasColumnName("currency_id");

		builder.Property(e => e.PurchasePrice).HasColumnName("price");

		builder.Property(e => e.DeliveryTime).HasColumnName("delivery_time");

		builder.Property(e => e.DeliveryProbability).HasColumnName("delivery_probability");

		builder.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at");

		builder.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");

		builder.Property(e => e.IsOpen).HasColumnName("is_open");
	}
}
