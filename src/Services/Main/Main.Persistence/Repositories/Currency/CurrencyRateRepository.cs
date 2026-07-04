using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Currency;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories.Currency;

public class CurrencyRateRepository(
    DContext context,
    IQueryableExtensions extensions
) : RepositoryBase<DContext, CurrencyRate, (int, int)>(context, extensions), ICurrencyRateRepository
{
    public Task<List<CurrencyRate>> GetByBaseCurrency(
        int baseCurrencyId,
        Criteria<CurrencyRate>? criteria = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.CurrencyRates
            .Where(x => x.ToCurrencyId == baseCurrencyId);
        query = criteria == null ? query : QueryableExtensions.Apply(query, criteria);
        return query.ToListAsync(cancellationToken);
    }

    public override Task<Dictionary<(int, int), CurrencyRate>> FindByIdsAsync(
        IEnumerable<(int, int)> ids,
        Criteria<CurrencyRate>? criteria = null,
        CancellationToken ct = default)
    {
        var keys = ids.Distinct().ToList();

        if (keys.Count == 0) return Task.FromResult(new Dictionary<(int, int), CurrencyRate>());

        var query = Context.CurrencyRates
            .AsExpandable();

        query = QueryableExtensions.Apply(query, criteria);

        var predicate = PredicateBuilder.New<CurrencyRate>();

        foreach (var (fromId, toId) in keys)
            predicate = predicate.Or(x =>
                x.FromCurrencyId == fromId &&
                x.ToCurrencyId == toId);

        return query
            .Where(predicate)
            .ToDictionaryAsync(
                x => (x.FromCurrencyId, x.ToCurrencyId),
                ct);
    }
}