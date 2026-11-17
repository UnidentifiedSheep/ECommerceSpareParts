using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Analytics.Persistence;

public sealed class AnalyticsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContextModule).Assembly);
}
