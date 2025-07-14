using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Sale;

public interface ISale
{
    Task<PostGres.Main.Sale> CreateSale(IEnumerable<NewSaleContentDto> sellContent,
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues,
        int currencyId, string buyerId, string createdUserId, string transactionId, string mainStorage,
        DateTime saleDateTime, string? comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет продажу из бд
    /// </summary>
    /// <param name="saleId">Id 'Продажи' которую надо удалить</param>
    /// <param name="whoDeletedUserId">Id пользователя, который удалил</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Удаленная сущность 'Продажа'</returns>
    Task<PostGres.Main.Sale> DeleteSale(string saleId, string whoDeletedUserId,
        CancellationToken cancellationToken = default);

    Task EditSale(IEnumerable<EditSaleContentDto> editedContent,
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues,
        Dictionary<int, List<SaleContentDetail>> movedToStorage,
        string saleId, int currencyId, string updatedUserId,
        DateTime saleDateTime, string? comment,
        CancellationToken cancellationToken = default);
}