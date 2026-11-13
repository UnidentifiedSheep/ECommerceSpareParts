using Domain.CommonEntities;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Currency.Persistence;

public sealed class CurrencyDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurrencyDbContextModule).Assembly);

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<CurrencySetting>(CurrencySetting.SettingName);
	}
}
