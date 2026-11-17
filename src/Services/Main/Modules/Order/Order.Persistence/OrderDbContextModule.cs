using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Order.Persistence;

public sealed class OrderDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContextModule).Assembly);
}
