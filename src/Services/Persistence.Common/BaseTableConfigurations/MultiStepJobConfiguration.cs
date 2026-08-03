using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public sealed class MultiStepJobConfiguration :
    IEntityTypeConfiguration<MultiStepJob>
{
    public void Configure(EntityTypeBuilder<MultiStepJob> builder)
    {
        builder.HasMany(x => x.Steps)
            .WithOne(x => x.MultiStepJob)
            .HasForeignKey(x => x.MultiStepJobId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("jobs_multi_step_job_id_fk");

        builder.Navigation(x => x.Steps)
            .HasField("_steps")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
