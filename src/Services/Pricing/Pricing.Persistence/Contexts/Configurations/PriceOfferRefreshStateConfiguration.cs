using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Contexts.Configurations;

public class PriceOfferRefreshStateConfiguration : IEntityTypeConfiguration<PriceOfferRefreshState>
{
    public void Configure(EntityTypeBuilder<PriceOfferRefreshState> builder)
    {
        builder.ToTable("price_offer_refresh_states", "public");

        builder.HasKey(e => new
            {
                e.ProductId,
                e.Source,
                e.StorageName
            })
            .HasName("price_offer_refresh_state_pk");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.Source)
            .HasMaxLength(64)
            .HasColumnName("source");

        builder.Property(e => e.StorageName)
            .HasMaxLength(128)
            .HasColumnName("storage_name");

        builder.Property(e => e.LastOffersUpdatedAt)
            .HasColumnName("last_offers_updated_at");

        builder.Property(e => e.LastOffersCount)
            .HasColumnName("last_offers_count");
    }
}
