using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Interceptors;
using Persistence.Interfaces;

namespace Persistence.Context;

public sealed class UnifiedDbContext(
	DbContextOptions<UnifiedDbContext> options,
	IEnumerable<IDbContextModule> modules) : DbContext(options)
{
	private readonly IDbContextModule[] _modules = modules.ToArray();

	internal string ModelModulesKey
		=> string.Join("|", _modules.Select(module => module.GetType().AssemblyQualifiedName));

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModuleModelCacheKeyFactory>();
		optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		foreach (var module in _modules) module.Configure(modelBuilder);
	}
}
