using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Purchase;

public class PurchasesFactConfiguration : IEntityTypeConfiguration<PurchasesFact>
{
    public void Configure(EntityTypeBuilder<PurchasesFact> builder)
    {
        builder.HasKey(e => e.Id).HasName("purchases_fact_pk");

        builder.ToTable("purchases_fact");

        builder.HasIndex(e => e.CurrencyId, "purchases_fact_currency_id_index");

        builder.HasIndex(e => e.SupplierId, "purchases_fact_supplier_id_index");

        builder.HasIndex(e => e.CreatedAt, "purchases_fact_created_at_index");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.SupplierId).HasColumnName("supplier_id");
        builder.Property(e => e.TotalSum).HasColumnName("total_sum");
    }
}
