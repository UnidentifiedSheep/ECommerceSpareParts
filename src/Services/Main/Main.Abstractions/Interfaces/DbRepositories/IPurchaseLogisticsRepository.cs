using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseLogisticsRepository
{
    Task<IEnumerable<PurchaseLogistic>> GetPurchaseLogistics(IEnumerable<string> ids, bool track = true,
        CancellationToken token = default);
}