using Application.Common.Interfaces.Repositories;
using Main.Entities.Currency;

namespace Main.Application.Interfaces.Persistence;

public interface ICurrencyRateRepository : IRepository<CurrencyRate, (int, int)>
{
    Task<List<CurrencyRate>> GetByBaseCurrency(
        int baseCurrencyId,
        Criteria<CurrencyRate>? criteria = null,
        CancellationToken cancellationToken = default);
}