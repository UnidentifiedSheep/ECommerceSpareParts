using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface ICurrencyRepository
{
    Task<Currency?> GetCurrency(int id, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> ids, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetAllCurrencies(bool track = true, CancellationToken cancellationToken = default);
}