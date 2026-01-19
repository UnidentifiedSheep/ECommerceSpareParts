using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface ICurrencyRepository
{
    Task<Dictionary<int, decimal>> GetCurrenciesToUsd(CancellationToken cancellationToken = default);
    Task<Currency?> GetCurrencyById(int id, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> exceptIds, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(int page, int limit, bool track = true,
        CancellationToken cancellationToken = default);
    Task<Currency?> GetCurrencyBeforeSpecifiedId(int id, bool track = true, CancellationToken cancellationToken = default);
}