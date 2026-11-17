using Main.Modules.Jobs.Persistence.BaseTableConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Jobs.Persistence;

public static class JobModelBuilderExtensions
{
	public static ModelBuilder ApplyJobConfigurations(this ModelBuilder modelBuilder)
		=> modelBuilder
			.ApplyConfiguration(new JobConfiguration())
			.ApplyConfiguration(new MultiStepJobConfiguration())
			.ApplyConfiguration(new JobStepDependencyConfiguration())
			.ApplyConfiguration(new JobScheduleConfiguration())
			.ApplyConfiguration(new JobScheduleRunConfiguration());
}
