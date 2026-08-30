using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.Common.BaseTableConfigurations;
using Persistence.Interceptors;
using Tests.Persistence.Entities;

namespace Tests.Persistence.Context;

/// <summary>
///     Test-only context for tables owned by the common application layer.
///     Service tests continue to use their own service DbContexts.
/// </summary>
internal sealed class DContext(DbContextOptions<DContext> options) : DbContext(options)
{
	public DbSet<Setting> Settings => Set<Setting>();

	public DbSet<Job> Jobs => Set<Job>();

	public DbSet<JobSchedule> JobSchedules => Set<JobSchedule>();

	public DbSet<JobScheduleRun> JobScheduleRuns => Set<JobScheduleRun>();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new SettingConfiguration()).ApplyJobConfigurations();

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<Setting>(nameof(Setting))
			.HasValue<TestSetting>(TestSetting.SettingName);

		modelBuilder.AddFieldsForAuditableEntities();
		modelBuilder.AllDateTimesToUtc().AllEnumsToString();
	}
}
