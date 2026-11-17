using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Persistence.Context;

public sealed class ModuleModelCacheKeyFactory : IModelCacheKeyFactory
{
	public object Create(DbContext context, bool designTime)
		=> context is UnifiedDbContext unified
			? (context.GetType(), unified.ModelModulesKey, designTime)
			: (context.GetType(), designTime);
}
