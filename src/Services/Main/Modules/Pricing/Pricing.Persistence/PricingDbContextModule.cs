using Domain.CommonEntities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Pricing.Entities.Settings;

namespace Pricing.Persistence;

public sealed class PricingDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContextModule).Assembly);

		modelBuilder
			.Entity<Setting>()
			.HasDiscriminator(e => e.Key)
			.HasValue<PricingSetting>(PricingSetting.SettingName);
	}
}
