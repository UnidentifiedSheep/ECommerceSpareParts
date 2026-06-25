using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.BaseTableConfigurations;

public class JobScheduleConfiguration : IEntityTypeConfiguration<JobSchedule>
{
    public void Configure(EntityTypeBuilder<JobSchedule> builder)
    {
        builder.ToTable("job_schedules");

        builder.HasKey(e => e.Id)
            .HasName("job_schedules_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(JobSchedule.NameMaxLength);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(JobSchedule.DescriptionMaxLength);

        builder.Property(e => e.JobSystemName)
            .HasColumnName("job_system_name")
            .HasMaxLength(128);

        builder.Property(e => e.InputState)
            .HasColumnName("input_state");

        builder.Property(e => e.MaxAttempts)
            .HasColumnName("max_attempts");

        builder.Property(e => e.Cron)
            .HasColumnName("cron")
            .HasMaxLength(128);

        builder.Property(e => e.Enabled)
            .HasColumnName("enabled");

        builder.Property(e => e.LastQueuedAt)
            .HasColumnName("last_queued_at");

        builder.Property(e => e.NextRunAt)
            .HasColumnName("next_run_at");

        builder.HasIndex(
            e => e.Name, 
            "job_schedules_name_idx");
        builder.HasIndex(
            e => e.JobSystemName, 
            "job_schedules_job_system_name_idx");
        builder.HasIndex(
            e => new { e.Enabled, e.NextRunAt, e.Id }, 
            "job_schedules_enabled_next_run_at_id_idx");
    }
}
