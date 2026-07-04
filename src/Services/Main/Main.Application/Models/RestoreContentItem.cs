using Main.Entities.Sale;

namespace Main.Application.Models;

public record RestoreContentItem(
    int StorageContentId,
    int ProductId,
    int CurrencyId,
    decimal BuyPrice,
    int Count
)
{
    public RestoreContentItem(SaleContentDetail detail, int productId)
        : this(
            detail.StorageContentId,
            productId,
            detail.CurrencyId,
            detail.BuyPrice,
            detail.Count)
    {
    }
}