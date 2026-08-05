using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Common.BaseTableConfigurations;

public sealed class JobStepDependencyConfiguration :
    IEntityTypeConfiguration<JobStepDependency>
{
    public void Configure(EntityTypeBuilder<JobStepDependency> builder)
    {
        builder.ToTable("job_step_dependencies", "job");

        builder.HasKey(x => new
            {
                x.StepId,
                x.DependsOnStepId
            })
            .HasName("job_step_dependencies_pk");

        builder.Property(x => x.StepId)
            .HasColumnName("step_id");

        builder.Property(x => x.MultiStepJobId)
            .HasColumnName("multi_step_job_id");

        builder.Property(x => x.DependsOnStepId)
            .HasColumnName("depends_on_step_id");

        builder.HasOne(x => x.Step)
            .WithMany()
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("job_step_dependencies_step_id_fk");

        builder.HasOne(x => x.DependsOnStep)
            .WithMany()
            .HasForeignKey(x => x.DependsOnStepId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("job_step_dependencies_depends_on_step_id_fk");

        builder.HasIndex(x => x.DependsOnStepId)
            .HasDatabaseName("job_step_dependencies_depends_on_step_id_idx");

        builder.HasIndex(x => x.MultiStepJobId)
            .HasDatabaseName("job_step_dependencies_multi_step_job_id_idx");
    }
}
