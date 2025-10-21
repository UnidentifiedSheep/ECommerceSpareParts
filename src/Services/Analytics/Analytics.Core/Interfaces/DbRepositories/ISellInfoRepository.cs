using System.Linq.Expressions;
using Analytics.Core.Entities;

namespace Analytics.Core.Interfaces.DbRepositories;

public interface ISellInfoRepository
{
    IAsyncEnumerable<SellInfo> GetSellInfos(Expression<Func<SellInfo, bool>> where, bool track = true);
    Task<SellInfo?> GetWithMaximumBuyPrice(bool track = true, CancellationToken cancellationToken = default);
}