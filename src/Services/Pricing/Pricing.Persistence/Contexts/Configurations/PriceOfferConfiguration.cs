using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;

namespace Pricing.Persistence.Contexts.Configurations;

public class PriceOfferConfiguration : IEntityTypeConfiguration<PriceOffer>
{
    public void Configure(EntityTypeBuilder<PriceOffer> builder)
    {
        builder.ToTable("price_offers", "public");
        
        builder.HasKey(e => e.Id)
            .HasName("price_offer_pk");

        builder.HasAlternateKey(e => new
            {
                e.Source,
                e.SourceKey,
                e.OfferForStorage
            })
            .HasName("price_offer_source_source_key_uq");

        builder.HasIndex(
            e => new { e.ProductId, e.OfferForStorage }, 
            "price_offer_product_id_index");

        builder.HasIndex(e => e.CurrencyId, "price_offer_currency_id_index");

        builder.HasIndex(e => e.OfferForStorage, "price_offer_offer_for_storage_index");

        builder.HasIndex(e => e.ExpiresAt, "price_offer_expires_at_index");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id");

        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(e => e.OfferForStorage)
            .HasMaxLength(128)
            .HasColumnName("offer_for_storage");

        builder.Property(e => e.Price)
            .HasColumnName("price");

        builder.Property(e => e.Source)
            .HasMaxLength(64)
            .HasColumnName("source");

        builder.Property(e => e.SourceKey)
            .HasMaxLength(256)
            .HasColumnName("source_key");

        builder.Property(e => e.AvailableQuantity)
            .HasColumnName("available_quantity");

        builder.Property(e => e.MinimumPurchaseQuantity)
            .HasColumnName("minimum_purchase_quantity");

        builder.Property(e => e.QuantityCoefficient)
            .HasColumnName("quantity_coefficient");

        builder.Property(e => e.DaysToRefund)
            .HasColumnName("days_to_refund");

        builder.Property(e => e.DeliveryDate)
            .HasColumnName("delivery_date");

        builder.Property(e => e.GuaranteedDeliveryDate)
            .HasColumnName("guaranteed_delivery_date");

        builder.Property(e => e.DeliveryProbability)
            .HasColumnName("delivery_probability");

        builder.Property(e => e.OrderTill)
            .HasColumnName("order_till");

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at");
    }
}
