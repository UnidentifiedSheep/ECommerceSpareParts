using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Currency;

public class CurrencyRepository(
    DContext context
) : RepositoryBase<DContext, Entities.Currency.Currency, int>(context), ICurrencyRepository
{
    public override Task<Dictionary<int, Entities.Currency.Currency>> FindByIdsAsync(
        IEnumerable<int> ids, 
        Criteria<Entities.Currency.Currency>? criteria = null, 
        CancellationToken ct = default)
    {
        return Context.Currencies
            .Where(x => ids.Contains(x.Id))
            .Apply(criteria)
            .ToDictionaryAsync(e => e.Id, ct);
    }
}