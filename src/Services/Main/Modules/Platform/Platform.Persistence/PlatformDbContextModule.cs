using Domain.CommonEntities;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Persistence.Common.BaseTableConfigurations;
using Persistence.Interfaces;

namespace Main.Modules.Platform.Persistence;

public sealed class PlatformDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new SettingConfiguration());

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<Setting>(nameof(Setting))
			.HasValue<GlobalApplicationSetting>(GlobalApplicationSetting.SettingName);
	}
}
