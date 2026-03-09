using System.Linq.Expressions;
using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface ISellInfoRepository
{
    IAsyncEnumerable<SellInfo> GetSellInfos(Expression<Func<SellInfo, bool>> where, bool track = true);
    Task<IEnumerable<SellInfo>> GetSellInfosList(Expression<Func<SellInfo, bool>> where, bool track = true, CancellationToken cancellationToken = default);
    Task<SellInfo?> GetWithMaximumBuyPrice(bool track = true, CancellationToken cancellationToken = default);
}