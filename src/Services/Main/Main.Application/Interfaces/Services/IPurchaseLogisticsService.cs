using Main.Entities.Purchase;

namespace Main.Application.Interfaces.Services;

public record PurchaseLogisticsItem(
    PurchaseContent PurchaseContent,
    int ProductId,
    int Quantity
);

public interface IPurchaseLogisticsService
{
    Task ApplyAsync(
        Purchase purchase,
        IEnumerable<PurchaseLogisticsItem> items,
        string? storageFrom,
        DateTime purchaseDateTime,
        Guid systemUserId,
        CancellationToken cancellationToken = default);
}