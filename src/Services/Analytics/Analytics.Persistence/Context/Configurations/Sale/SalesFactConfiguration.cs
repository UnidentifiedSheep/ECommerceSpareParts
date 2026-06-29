using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Sale;

public class SalesFactConfiguration : IEntityTypeConfiguration<SalesFact>
{
    public void Configure(EntityTypeBuilder<SalesFact> builder)
    {
        builder.HasKey(e => e.Id).HasName("sales_fact_pk");

        builder.ToTable("sales_fact");

        builder.HasIndex(e => e.BuyerId, "sales_fact_buyer_id_index");

        builder.HasIndex(e => e.CurrencyId, "sales_fact_currency_id_index");
        
        builder.HasIndex(e => e.CreatedAt, "sales_fact_created_at_index");

        builder.Property(e => e.Id)
            .HasMaxLength(128)
            .HasColumnName("id");
        builder.Property(e => e.BuyerId).HasColumnName("buyer_id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.TotalSum).HasColumnName("total_sum");

        builder.Navigation(e => e.SaleContents)
            .HasField("_saleContents")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
