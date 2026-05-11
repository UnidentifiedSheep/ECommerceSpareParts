using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Metric;

public class MetricConfiguration : IEntityTypeConfiguration<Entities.Metrics.Metric>
{
    public void Configure(EntityTypeBuilder<Entities.Metrics.Metric> builder)
    {
        builder.HasKey(e => e.Id).HasName("metrics_pk");

        builder.ToTable("metrics");

        builder.HasIndex(e => e.CurrencyId, "metrics_currency_id_index");

        builder.HasIndex(e => e.Discriminator, "metrics_dirty_index")
            .HasFilter("(tags & 1) = 1");

        builder.HasIndex(m => new { m.DependsOn, m.RangeStart, m.RangeEnd },
            "metrics_range_depends_index");

        builder.HasIndex(m => new { m.Discriminator, m.RangeStart, m.RangeEnd, m.DimensionHash },
                "metrics_range_start_end_discriminator_u_index")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("id");
        builder.Property(e => e.Tags)
            .HasColumnName("tags")
            .HasConversion<long>();

        builder.Property(m => m.DependsOn)
            .HasConversion<long>()
            .HasColumnName("depends_on");
        builder.Property(e => e.RecalculatedAt).HasColumnName("recalculated_at");
        builder.Property(e => e.CurrencyId).HasColumnName("currency_id");
        builder.Property(e => e.RangeStart).HasColumnName("range_start");
        builder.Property(e => e.RangeEnd).HasColumnName("range_end");
        builder.Property(e => e.Discriminator).HasColumnName("discriminator");
        builder.Property(e => e.DimensionKey)
            .HasColumnName("dimension_key")
            .HasMaxLength(200);
        builder.Property(e => e.DimensionHash).HasColumnName("dimension_hash")
            .HasColumnType("bytea");

        builder.Property(e => e.Json).HasColumnName("json");

        builder.HasDiscriminator(e => e.Discriminator);
    }
}