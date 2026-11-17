using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Balance.Persistence;

public sealed class BalanceDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(BalanceDbContextModule).Assembly);
}
