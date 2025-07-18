using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Sales;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.ArticleReservations;
using MonoliteUnicorn.Services.Balances;
using MonoliteUnicorn.Services.BuySellPriceService;
using MonoliteUnicorn.Services.Inventory;

namespace MonoliteUnicorn.Services.Sale;

public class SaleOrchestrator(IServiceProvider serviceProvider) : ISaleOrchestrator
{
    public async Task CreateFullSale(string createdUserId, string buyerId, int currencyId, string storageName, bool sellFromOtherStorages,
        DateTime saleDateTime, IEnumerable<NewSaleContentDto> saleContent, string? comment, decimal? payedSum, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var saleService = scope.ServiceProvider.GetRequiredService<ISale>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();
        var articleReservationService = scope.ServiceProvider.GetRequiredService<IArticleReservation>();
        var buySellPriceService = scope.ServiceProvider.GetRequiredService<IBuySellPriceService>();
        
        await context
            .WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var saleContentList = saleContent.ToList();
                foreach (var item in saleContentList)
                {
                    item.Price = Math.Round(item.Price, 2);
                    item.PriceWithDiscount = Math.Round(item.PriceWithDiscount, 2);
                }
                var totalSum = saleContentList.Sum(x => x.Count * x.PriceWithDiscount);
                var transaction = await balanceService.CreateTransactionAsync("SYSTEM", buyerId, totalSum, TransactionStatus.Sale, currencyId, createdUserId, saleDateTime, cancellationToken);
                var storageContentValues = await inventoryService.RemoveContentFromStorage(saleContentList.Select(x => (x.ArticleId, x.Count)), 
                    createdUserId, storageName, sellFromOtherStorages, StorageMovementType.Sale, cancellationToken);
                var storageContents = storageContentValues.ToList();
                var sale = await saleService.CreateSale(saleContentList, storageContents, currencyId, buyerId, 
                    createdUserId, transaction.Id, storageName, saleDateTime, comment, cancellationToken);
                
                if (payedSum is > 0)
                    await balanceService.CreateTransactionAsync(buyerId, "SYSTEM", payedSum.Value, TransactionStatus.Normal, 
                        currencyId, createdUserId, saleDateTime.AddMicroseconds(1), cancellationToken);
                
                await buySellPriceService.AddBuySellPrices(storageContents.Select(x => x.NewValue), 
                    sale.SaleContents, currencyId, cancellationToken);
                
                var saleCounts = sale.SaleContents
                    .GroupBy(x => x.ArticleId)
                    .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));

                await articleReservationService.SubtractCountFromReservations(buyerId, createdUserId, 
                    saleCounts, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }

    public async Task DeleteSale(string saleId, string userId, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var saleService = scope.ServiceProvider.GetRequiredService<ISale>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();

        await context
            .WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var sale = await saleService.DeleteSale(saleId, userId, cancellationToken);
                var transactionId = sale.TransactionId;
                var saleContentDetails = sale.SaleContents
                    .SelectMany(x => x.SaleContentDetails.Select(detail => (detail, x.ArticleId)))
                    .ToList();

                await balanceService.DeleteTransaction(transactionId, userId, cancellationToken);
                await inventoryService.RestoreContentToStorage(saleContentDetails, StorageMovementType.SaleDeletion,
                    userId, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }
    
    public async Task EditSale(IEnumerable<EditSaleContentDto> editedContent, string saleId,
        int currencyId, string updatedUserId, 
        DateTime saleDateTime, string? comment, bool sellFromOtherStorages,
        CancellationToken cancellationToken = default)
    {
        var editedContentList = editedContent.ToList();
        
        var seenIds = ValidateEditSaleInput(editedContentList);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var saleService = scope.ServiceProvider.GetRequiredService<ISale>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();
        var articleReservationService = scope.ServiceProvider.GetRequiredService<IArticleReservation>();
        var buySellPriceService = scope.ServiceProvider.GetRequiredService<IBuySellPriceService>();
        
        await context
            .WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var sale = await GetSaleForUpdateAsync(context, saleId, cancellationToken);
                var saleContent = await GetSaleContentForUpdateAsDictAsync(context, saleId, cancellationToken);
                var saleContentIds = saleContent.Keys;
                var saleContentDetails = await context.SaleContentDetails
                    .FromSql(
                        $"SELECT * FROM sale_content_details where sale_content_id = ANY({saleContentIds.ToArray()}) for update")
                    .ToListAsync(cancellationToken);

                var totalSum = editedContentList.Sum(x => x.Count * x.PriceWithDiscount);
                var (contentGreaterCount, contentLessCount) =
                    CalculateInventoryDeltas(editedContentList, saleContent, saleContentDetails);

                //Оставшиеся Id убираем из продажи
                contentLessCount.AddRange(GetRemovedContentDetails(seenIds, saleContent, saleContentDetails));
                //Возвращаем на склад
                if(contentLessCount.Count != 0)
                    await inventoryService.RestoreContentToStorage(contentLessCount, StorageMovementType.SaleEditing,
                        updatedUserId, cancellationToken);
                //Добавленные значения для позиций чьи количества были увеличены ЗАБИРАЕМ СО СКЛАДА
                var takenStorageContents = new List<PrevAndNewValue<StorageContent>>();
                if(contentGreaterCount.Count != 0)
                    takenStorageContents = (await inventoryService.RemoveContentFromStorage(contentGreaterCount,
                        updatedUserId, sale.MainStorageName, sellFromOtherStorages, StorageMovementType.SaleEditing,
                        cancellationToken)).ToList();
                
                //Редактируем транзакцию
                await balanceService.EditTransaction(sale.TransactionId, currencyId, totalSum, TransactionStatus.Sale,
                    saleDateTime, cancellationToken);
                //Редактируем саму продажу
                var movedToStorage = contentLessCount
                    .GroupBy(x => x.Item1.SaleContentId, x => x.Item1)
                    .ToDictionary(x => x.Key, x => x.ToList());
                await saleService.EditSale(editedContentList, takenStorageContents, movedToStorage, saleId, currencyId,
                    updatedUserId, saleDateTime, comment, cancellationToken);
                var toTakeFromReservations = contentGreaterCount
                    .GroupBy(x => x.ArticleId)
                    .ToDictionary(x => x.Key, 
                        x => x.Sum(z => z.Count));
                
                await articleReservationService.SubtractCountFromReservations(sale.BuyerId, updatedUserId,
                    toTakeFromReservations, cancellationToken);
                var newSaleContents = sale.SaleContents
                    .Where(x => !seenIds.Contains(x.Id))
                    .ToList();
                var oldSaleContents = sale.SaleContents
                    .Where(x => seenIds.Contains(x.Id))
                    .ToList();
                await buySellPriceService.AddBuySellPrices(takenStorageContents.Select(x => x.NewValue), 
                    newSaleContents, sale.CurrencyId, cancellationToken);
                await buySellPriceService.EditBuySellPrices(oldSaleContents, sale.CurrencyId, cancellationToken);
            }, cancellationToken);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="editedContentList"></param>
    /// <param name="saleContent"></param>
    /// <param name="saleContentDetails"></param>
    /// <param name="seenIds"></param>
    /// <returns>toTake - Содержимое у которых количество возросло ВЗЯТЬ СО СКЛАДА.
    /// toReturn - Содержимое у которых количество уменьшилось ВЕРНУТЬ НА СКОАД</returns>
    private (List<(int ArticleId, int Count)> toTake, List<(SaleContentDetail Detail, int ArticleId)> toReturn) CalculateInventoryDeltas(
            List<EditSaleContentDto> editedContentList,
            Dictionary<int, SaleContent> saleContent,
            List<SaleContentDetail> saleContentDetails)
    {
        var contentGreaterCount = new List<(int, int)>();
        var contentLessCount = new List<(SaleContentDetail, int)>();
        
        foreach (var content in editedContentList)
        {
            if (content.Id != null)
            {
                var existingSaleContent = saleContent[content.Id.Value];

                if (existingSaleContent.Count < content.Count)
                {
                    var diff = content.Count - existingSaleContent.Count;
                    contentGreaterCount.Add((existingSaleContent.ArticleId, diff));
                }
                else if (existingSaleContent.Count > content.Count)
                {
                    var diff = existingSaleContent.Count - content.Count;
                    var detailsQueue = new Queue<SaleContentDetail>(
                        saleContentDetails.Where(x => x.SaleContentId == content.Id));

                    while (diff > 0 && detailsQueue.Count > 0)
                    {
                        var detail = detailsQueue.Dequeue();
                        var tempCount = Math.Min(detail.Count, diff);
                        var newDetail = detail.Adapt<SaleContentDetail>();
                        newDetail.Count = tempCount;
                        diff -= tempCount;
                        contentLessCount.Add((newDetail, content.ArticleId));
                    }
                }
            }
            else
                contentGreaterCount.Add((content.ArticleId, content.Count));
            
        }

        return (contentGreaterCount, contentLessCount);
    }

    
    private List<(SaleContentDetail Detail, int ArticleId)> GetRemovedContentDetails(
        HashSet<int> seenIds,
        Dictionary<int, SaleContent> saleContent,
        List<SaleContentDetail> saleContentDetails)
    {
        var removedDetails = new List<(SaleContentDetail, int)>();

        foreach (var (id, deletedContent) in saleContent.Where(x => !seenIds.Contains(x.Key)))
        {
            var details = saleContentDetails
                .Where(x => x.SaleContentId == id).ToList();
            removedDetails.AddRange(details.Select(detail => (detail, deletedContent.ArticleId)));
        }

        return removedDetails;
    }

    
    private HashSet<int> ValidateEditSaleInput(IEnumerable<EditSaleContentDto> list)
    {
        var seenIds = new HashSet<int>();
        foreach (var item in list)
        {
            item.PriceWithDiscount = Math.Round(item.PriceWithDiscount, 2);
            item.Price = Math.Round(item.Price, 2);
            if (item.Count <= 0 || item.PriceWithDiscount <= 0 || item.Price <= 0)
                throw new SaleContentPriceOrCountException();
            if(item.Id == null) continue;
            if(!seenIds.Add(item.Id.Value)) throw new SameSaleContentException(item.Id.Value);
        }
        return seenIds;
    }

    private async Task<PostGres.Main.Sale> GetSaleForUpdateAsync(DContext context, string saleId, CancellationToken cancellationToken = default)
    {
        return await context.Sales
            .FromSql($"SELECT * FROM sale where id = {saleId} for update")
            .FirstOrDefaultAsync(cancellationToken) ?? throw new SaleNotFoundException(saleId);
    }

    private async Task<Dictionary<int, SaleContent>> GetSaleContentForUpdateAsDictAsync(DContext context, string saleId, CancellationToken cancellationToken = default)
    {
        return await context.SaleContents
            .FromSql($"SELECT * FROM sale_content where sale_id = {saleId} for update")
            .ToDictionaryAsync(x => x.Id, cancellationToken);
    }
}