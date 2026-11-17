using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Modules.Jobs.Persistence.BaseTableConfigurations;

public sealed class MultiStepJobConfiguration : IEntityTypeConfiguration<MultiStepJob>
{
	public void Configure(EntityTypeBuilder<MultiStepJob> builder)
	{
		builder.Navigation(x => x.Steps).HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);

		builder
			.HasMany(x => x.Dependencies)
			.WithOne(x => x.MultiStepJob)
			.HasForeignKey(x => x.MultiStepJobId)
			.OnDelete(DeleteBehavior.Cascade)
			.HasConstraintName("job_step_dependencies_multi_step_job_id_fk");

		builder
			.Navigation(x => x.Dependencies)
			.HasField("_dependencies")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}
