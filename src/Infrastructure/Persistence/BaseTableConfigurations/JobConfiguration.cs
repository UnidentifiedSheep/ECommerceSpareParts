using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.BaseTableConfigurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

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
            .HasColumnName("system_name");

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(e => e.State)
            .HasColumnName("state");
    }
}