using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Storage;

public class StorageRouteRepository(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, StorageRoute, Guid>(context, extensions), IStorageRouteRepository
{
	public async Task<StorageRoute?> GetActiveRouteAsync(
		string from,
		string to,
		Criteria<StorageRoute>? criteria = null,
		CancellationToken ct = default)
	{
		var query = Context.Set<StorageRoute>().AsQueryable();

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return await query.FirstOrDefaultAsync(
			x => x.FromStorageCode == from && x.ToStorageCode == to && x.IsActive,
			ct);
	}

	public async Task<bool> IsAnyRouteActiveAsync(
		string from,
		string to,
		CancellationToken ct = default)
		=> await Context.Set<StorageRoute>()
			.AnyAsync(
				x => x.FromStorageCode == from && x.ToStorageCode == to && x.IsActive,
				ct);
}
