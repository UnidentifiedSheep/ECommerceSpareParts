using Analytics.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Persistence.Context.Configurations.Metric;

public class MetricJobConfiguration : IEntityTypeConfiguration<MetricJob>
{
    public void Configure(EntityTypeBuilder<MetricJob> builder)
    {
        builder.ToTable("metric_jobs");

        builder.HasKey(e => new { e.MetricId, e.JobId })
            .HasName("metric_jobs_pk");

        builder.Property(e => e.MetricId)
            .HasColumnName("metric_id");

        builder.Property(e => e.JobId)
            .HasColumnName("job_id");

        builder.HasIndex(e => e.MetricId, "metric_jobs_metric_id_idx");
        builder.HasIndex(e => e.JobId, "metric_jobs_job_id_idx");

        builder.HasOne(e => e.Metric)
            .WithMany(e => e.Jobs)
            .HasForeignKey(e => e.MetricId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("metric_jobs_metric_id_fk");

        builder.HasOne(e => e.Job)
            .WithMany()
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("metric_jobs_job_id_fk");
    }
}