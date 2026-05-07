using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Currency;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories.Currency;

public class CurrencyRateRepository(
    DContext context
    ) 
    : RepositoryBase<DContext, CurrencyRate, (int, int)>(context), 
        ICurrencyRateRepository
{
    public Task<List<CurrencyRate>> GetByBaseCurrency(
        int baseCurrencyId,
        Criteria<CurrencyRate>? criteria = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.CurrencyRates
            .Where(x => x.ToCurrencyId == baseCurrencyId);
        query = criteria == null ? query : query.Apply(criteria);
        return query.ToListAsync(cancellationToken);
    }
}