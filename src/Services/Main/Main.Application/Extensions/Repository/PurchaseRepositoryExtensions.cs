using Application.Common.Interfaces.Repositories;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Extensions.Repository;

public static class PurchaseRepositoryExtensions
{
    public static async Task<Purchase> GetPurchaseForUpdate(
        this IRepository<Purchase, Guid> repository, 
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<Purchase>.New()
            .Where(x => x.Id == id)
            .Track()
            .ForUpdate()
            .Build();
        return await repository.FirstOrDefaultAsync(
            criteria,
            cancellationToken) ?? throw new PurchaseNotFoundException(id);
    }

    public static async Task<List<PurchaseContent>> GetPurchaseContents(
        this IRepository<PurchaseContent, int> repository,
        Guid purchaseId,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<PurchaseContent>.New()
            .Track()
            .Include(x => x.PurchaseContentLogistic)
            .Where(x => x.PurchaseId == purchaseId)
            .Build();
        
        return await repository.ListAsync(criteria, cancellationToken);
    }
}