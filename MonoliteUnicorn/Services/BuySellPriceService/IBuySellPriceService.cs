using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.BuySellPriceService;

public interface IBuySellPriceService
{
    Task AddBuySellPrices(IEnumerable<StorageContent> storageContents, IEnumerable<SaleContent> saleContents,
        int currencyId, CancellationToken cancellationToken = default);

    Task EditBuySellPrices(IEnumerable<SaleContent> saleContents, int currencyId,
        CancellationToken cancellationToken = default);
}