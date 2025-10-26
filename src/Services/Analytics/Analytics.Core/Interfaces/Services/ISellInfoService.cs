using Analytics.Core.Entities;
using Contracts.Models.Sale;

namespace Analytics.Core.Interfaces.Services;

public interface ISellInfoService
{
    /// <summary>
    /// Удаление sell infos по saleContentId
    /// </summary>
    /// <param name="saleContentIds">Id которые должны быть удалены</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<SellInfo>> RemoveSellInfos(IEnumerable<int> saleContentIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SellInfo>> CreateSellInfos(Sale sell, CancellationToken cancellationToken = default);
    Task<IEnumerable<SellInfo>> EditSellInfos(Sale sell, CancellationToken cancellationToken = default);
}