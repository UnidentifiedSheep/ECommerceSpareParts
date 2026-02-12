using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface ICurrencyRepository
{
    Task<Dictionary<int, decimal>> GetCurrenciesToUsd(CancellationToken cancellationToken = default);
    Task<Currency?> GetCurrencyById(int id, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> exceptIds, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(int? page = null, int? limit = null, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<Currency, object?>>[] includes);
    Task<Currency?> GetCurrencyBeforeSpecifiedId(int id, bool track = true, CancellationToken cancellationToken = default);
}