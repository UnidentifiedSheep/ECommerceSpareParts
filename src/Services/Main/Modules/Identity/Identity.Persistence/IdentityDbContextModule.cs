using Main.Modules.Identity.Persistence.DatabaseObjects;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Modules.Identity.Persistence;

public sealed class IdentityDbContextModule : IDbContextModule
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDbFunction(
				typeof(UserSearchDbFunctions)
					.GetMethod(nameof(UserSearchDbFunctions.ToUserSearchQuery))!)
			.HasName("user_search_query")
			.HasSchema("auth");

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContextModule).Assembly);
	}
}
