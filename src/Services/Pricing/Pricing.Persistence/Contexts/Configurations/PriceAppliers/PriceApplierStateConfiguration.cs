using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Pricing;

namespace Pricing.Persistence.Contexts.Configurations.PriceAppliers;

public class PriceApplierStateConfiguration : IEntityTypeConfiguration<PriceApplierState>
{
    public void Configure(EntityTypeBuilder<PriceApplierState> builder)
    {
        builder.ToTable("price_applier_states", "public");

        builder.HasKey(e => new
            {
                e.PriceApplierSystemName,
                e.Usage
            })
            .HasName("price_applier_states_pk");

        builder.Property(e => e.PriceApplierSystemName)
            .HasColumnName("price_applier_system_name");

        builder.Property(e => e.Usage)
            .HasColumnName("usage");

        builder.Property(e => e.Order)
            .HasColumnName("order");

        builder.Property(e => e.Enabled)
            .HasColumnName("enabled");
    }
}
