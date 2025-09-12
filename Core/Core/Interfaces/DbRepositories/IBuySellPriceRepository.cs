using System.Linq.Expressions;
using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IBuySellPriceRepository
{
    Task<IEnumerable<BuySellPrice>> GetBsPriceByContentIdsForUpdate(IEnumerable<int> contentIds, bool track = true, 
        CancellationToken cancellationToken = default);
    IAsyncEnumerable<BuySellPrice> GetBuySellPrices(Expression<Func<BuySellPrice,bool>>? where = null, bool track = true, CancellationToken cancellationToken = default);
    Task<List<BuySellPrice>> GetBuySellPricesAsList(Expression<Func<BuySellPrice,bool>>? where = null, bool track = true, CancellationToken cancellationToken = default);
    Task UpdateRange(IEnumerable<BuySellPrice> buySellPrices, CancellationToken cancellationToken = default);
    Task<decimal?> GetMaxBuyPrice(CancellationToken cancellationToken = default);
}