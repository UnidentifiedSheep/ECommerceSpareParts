using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public class UniqJobConfiguration : IEntityTypeConfiguration<UniqJob>
{
    public void Configure(EntityTypeBuilder<UniqJob> builder)
    {
        builder.Property(e => e.NaturalKey)
            .HasColumnName("natural_key")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(e => new
                {
                    e.SystemName,
                    e.NaturalKey
                },
                "jobs_pending_system_name_natural_key_uq")
            .IsUnique()
            .HasFilter("status = 'Pending'");
    }
}