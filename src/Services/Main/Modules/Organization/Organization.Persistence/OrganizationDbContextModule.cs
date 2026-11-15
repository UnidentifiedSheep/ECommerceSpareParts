using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Organization.Persistence;

public sealed class OrganizationDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
		=> modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContextModule).Assembly);
}
