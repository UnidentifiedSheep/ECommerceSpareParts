using Domain.CommonEntities;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Storage.Persistence;

public sealed class StorageDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(StorageDbContextModule).Assembly);

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<StorageContentSetting>(StorageContentSetting.SettingName);
	}
}
