using System.Linq.Expressions;
using Analytics.Core.Entities;

namespace Analytics.Core.Interfaces.DbRepositories;

public interface ISellInfoRepository
{
    IAsyncEnumerable<SellInfo> GetSellInfos(Expression<Func<SellInfo, bool>> where, bool track = true);
    Task<IEnumerable<SellInfo>> GetSellInfosList(Expression<Func<SellInfo, bool>> where, bool track = true, CancellationToken cancellationToken = default);
    Task<SellInfo?> GetWithMaximumBuyPrice(bool track = true, CancellationToken cancellationToken = default);
}