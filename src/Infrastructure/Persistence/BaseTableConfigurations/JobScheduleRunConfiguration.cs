using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.BaseTableConfigurations;

public class JobScheduleRunConfiguration : IEntityTypeConfiguration<JobScheduleRun>
{
    public void Configure(EntityTypeBuilder<JobScheduleRun> builder)
    {
        builder.ToTable("job_schedule_runs", "job");

        builder.HasKey(e => e.Id)
            .HasName("job_schedule_runs_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.JobScheduleId)
            .HasColumnName("job_schedule_id");

        builder.Property(e => e.JobId)
            .HasColumnName("job_id");

        builder.Property(e => e.ScheduledAt)
            .HasColumnName("scheduled_at");

        builder.Property(e => e.QueuedAt)
            .HasColumnName("queued_at");

        builder.HasIndex(
                e => new { e.JobScheduleId, e.ScheduledAt },
                "job_schedule_runs_job_schedule_id_scheduled_at_idx")
            .IsUnique();

        builder.HasIndex(
                e => e.JobId,
                "job_schedule_runs_job_id_idx")
            .IsUnique();

        builder.HasOne<JobSchedule>()
            .WithMany()
            .HasForeignKey(e => e.JobScheduleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("job_schedule_runs_job_schedule_id_fk");

        builder.HasOne<Job>()
            .WithMany()
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("job_schedule_runs_job_id_fk");
    }
}
