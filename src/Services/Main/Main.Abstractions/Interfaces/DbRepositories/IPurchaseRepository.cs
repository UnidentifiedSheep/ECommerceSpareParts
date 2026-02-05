using System.Linq.Expressions;
using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IPurchaseRepository
{
    Task<Purchase?> GetPurchaseForUpdate(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PurchaseContent>> GetPurchaseContentForUpdate(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Purchase>> GetPurchases(DateTime rangeStart, DateTime rangeEnd, int page, int viewCount,
        Guid? supplierId,
        int? currencyId, string? sortBy, string? searchTerm, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PurchaseContent>> GetPurchaseContent(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default, params Expression<Func<PurchaseContent, object?>>[] includes);
}