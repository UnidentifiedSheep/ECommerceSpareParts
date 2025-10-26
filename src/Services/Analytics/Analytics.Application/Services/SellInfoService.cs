using Analytics.Core.Entities;
using Analytics.Core.Interfaces.DbRepositories;
using Analytics.Core.Interfaces.Services;
using Core.Interfaces.Services;

namespace Analytics.Application.Services;

public class SellInfoService(IUnitOfWork unitOfWork, ISellInfoRepository sellInfoRepository) : ISellInfoService
{
    public async Task<IEnumerable<SellInfo>> RemoveSellInfos(IEnumerable<int> saleContentIds,
        CancellationToken cancellationToken = default)
    {
        var toRemoveSet = new HashSet<int>(saleContentIds);
        if (toRemoveSet.Count == 0) return [];
        
        var neededToBeRemoved = (await sellInfoRepository
            .GetSellInfosList(x => toRemoveSet.Contains(x.SellContentId))).ToList();
        unitOfWork.RemoveRange(neededToBeRemoved);
        return neededToBeRemoved;
    }

    public async Task<IEnumerable<SellInfo>> CreateSellInfos()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<SellInfo>> EditSellInfos()
    {
        throw new NotImplementedException();
    }
}