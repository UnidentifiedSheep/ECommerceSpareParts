using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public sealed class JobStepConfiguration :
    IEntityTypeConfiguration<JobStep>
{
    public void Configure(EntityTypeBuilder<JobStep> builder)
    {
        builder.Property(x => x.MultiStepJobId)
            .HasColumnName("multi_step_job_id");

        builder.HasIndex(x => x.MultiStepJobId)
            .HasDatabaseName("jobs_multi_step_job_id_idx");

        builder.HasMany(x => x.Dependencies)
            .WithOne(x => x.Step)
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("job_step_dependencies_step_id_fk");

        builder.Navigation(x => x.Dependencies)
            .HasField("_dependencies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
