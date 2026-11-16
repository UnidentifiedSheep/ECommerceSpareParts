using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Relations;

public sealed class RelationsDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelationsDbContextModule).Assembly);
}
