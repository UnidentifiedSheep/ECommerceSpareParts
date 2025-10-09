using Core.Entities;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

namespace Persistence.Repositories;

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

    public async Task<bool> CurrencyExists(int id, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<int>> CurrenciesExists(IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idsSet = new HashSet<int>(ids);
        var foundIds = await context.Currencies.AsNoTracking()
            .Where(x => idsSet.Contains(x.Id))
            .Select(x => x.Id).ToHashSetAsync(cancellationToken);
        return idsSet.Except(foundIds);
    }

    public async Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> exceptIds, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .Include(x => x.CurrencyToUsd)
            .Where(x => !exceptIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCurrencyCodeTaken(string code, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.AsNoTracking().AnyAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<bool> IsCurrencyShortNameTaken(string shortName, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.AsNoTracking().AnyAsync(x => x.ShortName == shortName, cancellationToken);
    }

    public async Task<bool> IsCurrencyNameTaken(string name, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.AsNoTracking().AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<bool> IsCurrencySignTaken(string sign, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.AsNoTracking().AnyAsync(x => x.CurrencySign == sign, cancellationToken);
    }

    public async Task<IEnumerable<Currency>> GetCurrencies(int page, int limit, bool track = true, CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .Skip(page * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}