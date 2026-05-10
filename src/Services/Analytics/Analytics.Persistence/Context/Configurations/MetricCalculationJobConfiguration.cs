using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations;

public class MetricCalculationJobConfiguration : IEntityTypeConfiguration<MetricCalculationJob>
{
    public void Configure(EntityTypeBuilder<MetricCalculationJob> builder)
    {
        builder.ToTable("metric_calculation_jobs");

        builder.HasKey(e => e.RequestId)
            .HasName("request_id_pk");

        builder.Property(e => e.RequestId)
            .HasDefaultValueSql("gen_random_uuid()")
            .HasColumnName("request_id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.MetricId)
            .HasColumnName("metric_id");

        builder.Property(e => e.MetricSystemName)
            .HasColumnName("metric_system_name")
            .HasMaxLength(128);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(512);

        builder.Property(e => e.RowVersion)
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.HasIndex(e => e.MetricId, "metrics_calc_jobs_metric_id_index")
            .IsUnique();

        builder.HasIndex(e =>
                new { e.Status, e.MetricSystemName },
            "metrics_calc_jobs_status_name_index");
    }
}