using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Entities.Pricing;

namespace Pricing.Persistence.Contexts.Configurations.PriceAppliers;

public class PriceApplierConfiguration : IEntityTypeConfiguration<PriceApplier>
{
    public void Configure(EntityTypeBuilder<PriceApplier> builder)
    {
        builder.ToTable("price_appliers", "public");

        builder.HasKey(e => e.SystemName)
            .HasName("price_appliers_pk");

        builder.Property(e => e.SystemName)
            .ValueGeneratedNever()
            .HasColumnName("system_name");

        builder.Property(e => e.DslLogic)
            .HasColumnType("jsonb")
            .HasColumnName("dsl_logic");

        builder.HasMany(e => e.States)
            .WithOne()
            .HasForeignKey(e => e.PriceApplierSystemName)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("price_applier_states_price_applier_system_name_fk");

        builder.Navigation(e => e.States)
            .HasField("_states")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
