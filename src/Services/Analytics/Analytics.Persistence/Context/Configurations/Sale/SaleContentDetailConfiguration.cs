using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Sale;

public class SaleContentDetailConfiguration : IEntityTypeConfiguration<SaleContentDetail>
{
    public void Configure(EntityTypeBuilder<SaleContentDetail> builder)
    {
        builder.HasKey(e => e.Id).HasName("sale_content_detail_pk");

        builder.ToTable("sale_content_detail");

        builder.HasIndex(e => e.CurrencyId, "sale_content_detail_currency_id_index");
        builder.HasIndex(e => e.SaleContentId, "sale_content_detail_sale_content_id_index");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");
        builder.Property(e => e.SaleContentId).HasColumnName("sale_content_id");
        builder.Property(e => e.BuyPrice).HasColumnName("buy_price");
        builder.Property(e => e.BuyPriceInBaseCurrency).HasColumnName("buy_price_in_base_currency");
        builder.Property(e => e.Count).HasColumnName("count");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
    }
}