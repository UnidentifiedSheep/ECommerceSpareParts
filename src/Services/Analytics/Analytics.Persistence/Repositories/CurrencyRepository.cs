using Analytics.Core.Entities;
using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class CurrencyRepository(DContext context) : ICurrencyRepository
{
    public async Task<Currency?> GetCurrency(int id, bool track = true, CancellationToken cancellationToken = default)
        => await context.Currencies.ConfigureTracking(track).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> ids, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies.ConfigureTracking(track)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}