using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Product.Persistence;

public sealed class ProductDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContextModule).Assembly);
}
