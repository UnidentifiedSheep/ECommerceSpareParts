using MonoliteUnicorn.Dtos.Amw.Sales;

namespace MonoliteUnicorn.Services.Sale;

public interface ISaleOrchestrator
{
    Task CreateFullSale(string createdUserId, string buyerId, int currencyId, string? storageName, bool sellFromOtherStorages,
        DateTime saleDateTime, IEnumerable<NewSaleContentDto> saleContent, string? comment, decimal? payedSum,
        CancellationToken cancellationToken = default);

    Task DeleteSale(string saleId, string userId, CancellationToken cancellationToken = default);

    Task EditSale(IEnumerable<EditSaleContentDto> editedContent, string saleId,
        int currencyId, string updatedUserId,
        DateTime saleDateTime, string? comment, bool sellFromOtherStorages,
        CancellationToken cancellationToken = default);
}