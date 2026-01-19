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

    public async Task<IEnumerable<Currency>> GetCurrencies(int page, int limit, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .OrderBy(x => x.Id)
            .Skip(page * limit)
            .Take(limit)
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