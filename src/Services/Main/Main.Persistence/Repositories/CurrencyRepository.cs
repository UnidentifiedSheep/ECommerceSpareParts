using System.Linq.Expressions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class CurrencyRepository(DContext context) : ICurrencyRepository
{
    public async Task<Dictionary<int, decimal>> GetCurrenciesToUsd(CancellationToken cancellationToken = default)
    {
        return await context.CurrencyToUsds.AsNoTracking()
            .ToDictionaryAsync(x => x.CurrencyId, x => x.ToUsd, cancellationToken);
    }

    public async Task<Currency?> GetCurrencyById(int id, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> exceptIds, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .Include(x => x.CurrencyToUsd)
            .Where(x => !exceptIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Currency>> GetCurrencies(int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<Currency, object?>>[] includes)
    {
        var query = context.Currencies.ConfigureTracking(track);

        if (page.HasValue && limit.HasValue)
            query = query.Skip(page.Value * limit.Value).Take(limit.Value);
        
        foreach (var include in includes)
            query = query.Include(include);
        
        return await query
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
    }

    public async Task<Currency?> GetCurrencyBeforeSpecifiedId(int id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .Where(x => x.Id < id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}