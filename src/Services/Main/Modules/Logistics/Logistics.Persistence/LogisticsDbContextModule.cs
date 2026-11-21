using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Logistics.Persistence;

public sealed class LogisticsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogisticsDbContextModule).Assembly);
}
