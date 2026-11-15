using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Cart.Persistence;

public sealed class CartDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContextModule).Assembly);
}
