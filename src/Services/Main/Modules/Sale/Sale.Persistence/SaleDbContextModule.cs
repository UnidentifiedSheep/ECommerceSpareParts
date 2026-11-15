using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Sale.Persistence;

public sealed class SaleDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaleDbContextModule).Assembly);
}
