using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities;
using Pricing.Entities.Offers;

namespace Pricing.Persistence.Contexts.Configurations;

public class ProductPriceOptionConfiguration : IEntityTypeConfiguration<ProductPriceOption>
{
    public void Configure(EntityTypeBuilder<ProductPriceOption> builder)
    {
        builder.ToTable("product_price_options", "public");

        builder.HasKey(e => e.PriceOfferId)
            .HasName("product_price_option_pk");

        builder.HasIndex(e => e.CurrencyId, "product_price_option_currency_id_index");

        builder.HasIndex(e => e.Score, "product_price_option_score_index");

        builder.Property(e => e.PriceOfferId)
            .HasColumnName("price_offer_id");

        builder.Property(e => e.Score)
            .HasColumnName("score");

        builder.Property(e => e.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(e => e.Price)
            .HasColumnName("price");
        
        builder.Property(x => x.MarkupVersion)
            .HasColumnName("markup_version");

        builder.Property(x => x.AppliersVersion)
            .HasColumnName("appliers_version");

        builder.Property(x => x.PricingSettingsVersion)
            .HasColumnName("pricing_settings_version");

        builder.Property(e => e.Markup)
            .HasColumnName("markup");

        builder.Property(e => e.ForStorageName)
            .HasMaxLength(128)
            .HasColumnName("for_storage_name");

        builder.Property(e => e.DeliveryTime)
            .HasColumnName("delivery_time");

        builder.Property(e => e.GuaranteedDeliveryTime)
            .HasColumnName("guaranteed_delivery_time");

        builder.Property(e => e.DeliveryProbability)
            .HasColumnName("delivery_probability");

        builder.HasOne(e => e.PriceOffer)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade)
            .HasForeignKey<ProductPriceOption>(e => e.PriceOfferId)
            .HasConstraintName("product_price_options_price_offer_id_fk");
    }
}
