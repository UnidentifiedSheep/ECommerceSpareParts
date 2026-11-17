using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Purchase.Persistence;

public sealed class PurchaseDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(PurchaseDbContextModule).Assembly);
}
