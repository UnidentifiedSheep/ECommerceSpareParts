using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface ICurrencyRepository
{
    Task<Dictionary<int, decimal>> GetCurrenciesToUsd(CancellationToken cancellationToken = default);
    Task<Currency?> GetCurrencyById(int id, bool track = true, CancellationToken cancellationToken = default);
    Task<bool> CurrencyExists(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Проверка на то существуют ли валюты.
    /// </summary>
    /// <param name="ids">id валют</param>
    /// <param name="cancellationToken">токен отмены операции</param>
    /// <returns>ids which not exists</returns>
    Task<IEnumerable<int>> CurrenciesExists(IEnumerable<int> ids, CancellationToken cancellationToken = default);

    Task<IEnumerable<Currency>> GetCurrencies(IEnumerable<int> exceptIds, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> IsCurrencyCodeTaken(string code, CancellationToken cancellationToken = default);
    Task<bool> IsCurrencyShortNameTaken(string shortName, CancellationToken cancellationToken = default);
    Task<bool> IsCurrencyNameTaken(string name, CancellationToken cancellationToken = default);
    Task<bool> IsCurrencySignTaken(string sign, CancellationToken cancellationToken = default);
}