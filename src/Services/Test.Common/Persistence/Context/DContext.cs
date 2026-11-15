using Domain.CommonEntities;
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
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new SettingConfiguration());

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<Setting>(nameof(Setting))
			.HasValue<TestSetting>(TestSetting.SettingName);

		modelBuilder.Entity<TestMetadataEntity>().ToTable("metadata_entities").HasKey(entity => entity.Id);
		modelBuilder.Entity<TestAuditableMetadataEntity>()
			.ToTable("auditable_metadata_entities")
			.HasKey(entity => entity.Id);

		modelBuilder.ApplyConfiguration(new EntityHistoryConfiguration());
		modelBuilder.Entity<BeforeSaveTestEntity>()
			.ToTable("before_save_entities")
			.HasKey(entity => entity.Id);
		modelBuilder.Entity<BeforeSaveTestEntity>().Ignore(entity => entity.CreatedCalls);
		modelBuilder.ApplyMetadataConfigurations();
		modelBuilder.AddFieldsForAuditableEntities();
		modelBuilder.AllDateTimesToUtc().AllEnumsToString();
	}
}
