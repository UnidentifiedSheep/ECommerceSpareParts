using MonoliteUnicorn.Dtos.Amw.Purchase;

namespace MonoliteUnicorn.Services.Purchase;

public interface IPurchaseOrchestrator
{
    Task CreateFullPurchase(string createdUserId, string supplierId, int currencyId, string storageName,
        DateTime purchaseDate,
        IEnumerable<NewPurchaseContentDto> purchaseContent, string? comment, decimal? payedSum,
        CancellationToken cancellationToken = default);

    Task EditPurchase(IEnumerable<EditPurchaseDto> content, string purchaseId, int currencyId, string? comment,
        string updatedUserId, DateTime purchaseDateTime, CancellationToken cancellationToken = default);

    Task DeletePurchase(string purchaseId, string userId, CancellationToken cancellationToken = default);
}