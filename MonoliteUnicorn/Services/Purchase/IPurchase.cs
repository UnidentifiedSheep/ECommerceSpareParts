using MonoliteUnicorn.Dtos.Amw.Purchase;

namespace MonoliteUnicorn.Services.Purchase;

public interface IPurchase
{
    Task<PostGres.Main.Purchase> CreatePurchaseAsync(IEnumerable<(int ArticleId, int Count, decimal Price, string? Comment)> content, int currencyId,
        string createdUserId, string transactionId, string storageName,
        string supplierId, DateTime purchaseDateTime, string? comment = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Изменение существующей закупки.
    /// </summary>
    /// <param name="content">Содержимое закупки</param>
    /// <param name="purchaseId">Айди закупки</param>
    /// <param name="currencyId">Айди валюты</param>
    /// <param name="comment">Комментарий</param>
    /// <param name="updatedUserId">Пользователь, который обновил закупку</param>
    /// <param name="purchaseDateTime">Дата закупки</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Возвращает список где Key - айди артикула, далее словарь где Key - цена и Value - количество.
    /// Если количество отрицательное, то это количество, которое взяли со склада. Если положительное то вернули на склад.</returns>
    Task<Dictionary<int, Dictionary<decimal, int>>> EditPurchaseAsync(IEnumerable<EditPurchaseDto> content, string purchaseId, int currencyId, string? comment,
        string updatedUserId, DateTime purchaseDateTime, CancellationToken cancellationToken = default);

    Task DeletePurchase(string purchaseId, CancellationToken cancellationToken = default);
}