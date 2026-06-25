using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.BaseTableConfigurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs", "job");

        builder.HasKey(e => e.Id)
            .HasName("jobs_pk");

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>();
        
        builder.Property(e => e.Attempts)
            .HasColumnName("attempts");
        
        builder.Property(e => e.MaxAttempts)
            .HasColumnName("max_attempts");

        builder.Property(e => e.SystemName)
            .HasColumnName("system_name")
            .HasMaxLength(128);

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(e => e.LockedAt)
            .HasColumnName("locked_at");
        
        builder.Property(e => e.State)
            .HasColumnName("state");

        builder.HasIndex(e => e.SystemName, "jobs_system_name_idx");
        builder.HasIndex(e => new { e.Status, e.Id }, "jobs_status_id_idx");
        builder.HasIndex(e => e.LockedAt, "jobs_locked_at_idx");
    }
}