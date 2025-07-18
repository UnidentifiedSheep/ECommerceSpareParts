using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.Services.Inventory;

public class Inventory(DContext context) : IInventory
{
    public async Task<IEnumerable<StorageContent>> AddContentToStorage(IEnumerable<(int ArticleId, int Count, decimal Price, int currencyId)> content, 
        string storageName, string userId, StorageMovementType movementType, 
        CancellationToken cancellationToken = default)
    {
        var contentList = content.ToList();
        if (contentList.Count == 0)
            throw new ArgumentException("Список content пустой");
        if (contentList.Any(x => Math.Round(x.Price, 2) <= 0))
            throw new StorageContentPriceCannotBeNegativeException();
        if (contentList.Any(x => x.Count <= 0))
            throw new StorageContentCountCantBeNegativeException();
        
        var currencyIds = contentList.Select(x => x.currencyId).ToHashSet();
        
        await context.EnsureCurrenciesExist(currencyIds, cancellationToken);
        await context.EnsureStorageExists(storageName, cancellationToken);
        await context.EnsureUserExists(userId, cancellationToken);
        
        return await context
            .WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
        {
            var articleIds = contentList
                .Select(x => x.ArticleId).ToHashSet();
            //Блокируем артикулы для конкурентного обновления количества
            _ = await context.EnsureArticlesExistForUpdate(articleIds, cancellationToken);
            
            var toIncrement = new Dictionary<int, int>();

            var storageContents = new List<StorageContent>();
            var storageMovements = new List<StorageMovement>();
            foreach (var (articleId, count, price, currencyId) in contentList)
            {
                var storageContent = new StorageContent
                {
                    StorageName = storageName,
                    ArticleId = articleId,
                    BuyPrice = price,
                    CurrencyId = currencyId,
                    BuyPriceInUsd = CurrencyConverter.ConvertToUsd(price, currencyId),
                    PurchaseDatetime = DateTime.Now,
                    Count = count
                };
                storageContents.Add(storageContent);
                var storageMovement = storageContent
                    .Adapt<StorageMovement>()
                    .SetActionType(movementType);
                storageMovement.WhoMoved = userId;
                storageMovements.Add(storageMovement);
                
                if(!toIncrement.TryAdd(articleId, count))
                    toIncrement[articleId] += count;
            }
            
            var expectedCount = toIncrement.Count;
            var updatedRows = await context.UpdateArticlesCount(toIncrement, cancellationToken);
            
            if (updatedRows != expectedCount)
                throw new InvalidOperationException("Не все артикулы найдены для обновления");
            
            await context.StorageMovements.AddRangeAsync(storageMovements, cancellationToken);
            await context.StorageContents.AddRangeAsync(storageContents, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return storageContents;
        }, cancellationToken);
    }

    public async Task RestoreContentToStorage(IEnumerable<(SaleContentDetail, int)> contentDetails, StorageMovementType movementType, string userId,
        CancellationToken cancellationToken = default)
    {
        var contentDetailsList = contentDetails.ToList();
        
        if (contentDetailsList.Count == 0)
            throw new ArgumentException("Список content пустой");
        if (contentDetailsList.Any(x => Math.Round(x.Item1.BuyPrice, 2) <= 0))
            throw new StorageContentPriceCannotBeNegativeException();
        if (contentDetailsList.Any(x => x.Item1.Count <= 0))
            throw new StorageContentCountCantBeNegativeException();
        
        var currencyIds = contentDetailsList.Select(x => x.Item1.CurrencyId).ToHashSet();
        await context.EnsureCurrenciesExist(currencyIds, cancellationToken);
        await context.EnsureUserExists(userId, cancellationToken);
        var storageIds = contentDetailsList
            .Select(x => x.Item1.Storage)
            .ToHashSet();
        await context.EnsureStoragesExist(storageIds, cancellationToken);
        
        await context
            .WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var articleIds = contentDetailsList
                .Select(x => x.Item2)
                .ToHashSet();
            
                //Блокируем артикулы для конкурентного обновления количества
                _ = await context.EnsureArticlesExistForUpdate(articleIds, cancellationToken);
            
                var toIncrement = new Dictionary<int, int>();
                
                foreach (var (contentDetail, articleId) in contentDetailsList)
                {
                    var storageContent = await context.StorageContents.FromSql($"""
                                                                                SELECT * 
                                                                                FROM storage_content 
                                                                                where id = {contentDetail.StorageContentId} and 
                                                                                      article_id = {articleId} and 
                                                                                      storage_name = {contentDetail.Storage}
                                                                                for update
                                                                                """).FirstOrDefaultAsync(cancellationToken);
                    if (storageContent != null)
                        storageContent.Count += contentDetail.Count;
                    else
                    {
                        storageContent = contentDetail.Adapt<StorageContent>();
                        storageContent.BuyPriceInUsd = CurrencyConverter.ConvertToUsd(storageContent.BuyPrice, storageContent.CurrencyId);
                        storageContent.ArticleId = articleId;
                        await context.AddAsync(storageContent, cancellationToken);
                    }
                    var tempMovement = storageContent.Adapt<StorageMovement>()
                        .SetActionType(movementType);
                    tempMovement.Count = contentDetail.Count;
                    tempMovement.WhoMoved = userId;
                    await context.StorageMovements.AddAsync(tempMovement, cancellationToken);
                    
                    if(!toIncrement.TryAdd(articleId, contentDetail.Count))
                        toIncrement[articleId] += contentDetail.Count;
                }
                
                var expectedCount = toIncrement.Count;
                var updatedRows = await context.UpdateArticlesCount(toIncrement, cancellationToken);
            
                if (updatedRows != expectedCount)
                    throw new InvalidOperationException("Не все артикулы найдены для обновления");
                
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }

    public async Task DeleteContentFromStorage(int contentId, string userId, StorageMovementType movementType,
        CancellationToken cancellationToken = default)
    {
        await context.EnsureUserExists(userId, cancellationToken);
        await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () => 
            {
                var content = await context.StorageContents
                    .FromSql($"select * from storage_content where id = {contentId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageContentNotFoundException(contentId);

                var article = await context.Articles
                    .FromSql($"select * from articles where id = {content.ArticleId} for update")
                    .FirstAsync(cancellationToken);
                
                var tempMovement = content.Adapt<StorageMovement>().SetActionType(movementType);
                tempMovement.Count = content.Count;
                tempMovement.WhoMoved = userId;
                await context.StorageMovements.AddAsync(tempMovement, cancellationToken);
                
                article.TotalCount -= content.Count;
                context.StorageContents.Remove(content);

                await context.SaveChangesAsync(cancellationToken); 
            }, cancellationToken);
    }

    public async Task<IEnumerable<PrevAndNewValue<StorageContent>>> RemoveContentFromStorage(IEnumerable<(int ArticleId, int Count)> content, 
        string userId, string? storageName, bool takeFromOtherStorages, StorageMovementType movementType, CancellationToken cancellationToken = default)
    {
        var contentList = content.ToList();
        if(contentList.Count == 0)
            throw new ArgumentException("Пустой content список");
        if (!takeFromOtherStorages && string.IsNullOrWhiteSpace(storageName)) 
            throw new StorageIsUnknownException();
        if(!takeFromOtherStorages && !string.IsNullOrWhiteSpace(storageName))
            await context.EnsureStorageExists(storageName, cancellationToken);
        
        await context.EnsureUserExists(userId, cancellationToken);
        
        return await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var articleIds = contentList.Select(x => x.ArticleId);
                //Блокируем артикулы для конкурентного обновления количества
                _ = await context.EnsureArticlesExistForUpdate(articleIds, cancellationToken);
                
                var toIncrement = new Dictionary<int, int>();
                var result = new List<PrevAndNewValue<StorageContent>>();
                var mergedContent = new Dictionary<int, int>();

                foreach (var ck in contentList)
                {
                    if (ck.Count <= 0) 
                        throw new ArgumentException("Количество для удаления со склада не может быть отрицательным или 0");
                    if (!mergedContent.TryAdd(ck.ArticleId, ck.Count))
                        mergedContent[ck.ArticleId] += ck.Count;
                }
                foreach (var (articleId, count) in mergedContent)
                {
                    List<StorageContent> storageContents = [];
                    
                    int availableCount = 0;
                    if (!string.IsNullOrWhiteSpace(storageName))
                    {
                        await foreach (var item in context.StorageContents
                                           .FromSql($"""
                                                     select * from storage_content 
                                                     where article_id = {articleId}
                                                       and storage_name = {storageName} 
                                                       and count > 0
                                                     ORDER BY purchase_datetime ASC, count DESC
                                                     for update
                                                     """).AsAsyncEnumerable().WithCancellation(cancellationToken))
                        {
                            storageContents.Add(item);
                            availableCount += item.Count;
                            if (availableCount >= count) break;
                        }
                    }

                    if (takeFromOtherStorages)
                    {
                        await foreach (var item in context.StorageContents
                                           .FromSql($"""
                                                      SELECT * 
                                                      FROM storage_content
                                                      WHERE article_id = {articleId}
                                                        AND (storage_name != {storageName} OR {string.IsNullOrWhiteSpace(storageName)})
                                                        AND count > 0
                                                      ORDER BY purchase_datetime ASC, count DESC
                                                      FOR UPDATE
                                                      """).AsAsyncEnumerable().WithCancellation(cancellationToken))
                        {
                            if (availableCount >= count) break;
                            storageContents.Add(item);
                            availableCount += item.Count;
                        }
                    }
                    if (availableCount < count) throw new NotEnoughCountOnStorageException(articleId, availableCount);
                    int counter = count;
                    
                    foreach (var item in storageContents)
                    {
                        var prevValue = item.Adapt<StorageContent>();
                        var temp = Math.Min(counter, item.Count);
                        item.Count -= temp;
                        counter -= temp;
                        var newValue = item.Adapt<StorageContent>();
                        
                        var tempMovement = item.Adapt<StorageMovement>().SetActionType(movementType);
                        tempMovement.Count = -temp;
                        tempMovement.WhoMoved = userId;
                        await context.StorageMovements.AddAsync(tempMovement, cancellationToken);
                        
                        result.Add(new (prevValue, newValue));
                        if (counter <= 0) break;
                    }
                    if(!toIncrement.TryAdd(articleId, -count))
                        toIncrement[articleId] -= count;
                }
                
                var expectedCount = toIncrement.Count;
                var updatedRows = await context.UpdateArticlesCount(toIncrement, cancellationToken);
                
                if (updatedRows != expectedCount)
                    throw new InvalidOperationException("Не все артикулы найдены для обновления");
                
                await context.SaveChangesAsync(cancellationToken);
                return result;
            }, cancellationToken);
    }

    public async Task AddOrRemoveContentFromStorage(Dictionary<int, Dictionary<decimal, int>> addRemoveDict, int currencyId, string storageName,
        DateTime prevPurchaseDateTime, DateTime newPurchaseDateTime, string userId,
        StorageMovementType movementType, CancellationToken cancellationToken = default)
    {
        if (addRemoveDict.Count == 0) return;
        await context.EnsureUserExists(userId, cancellationToken);
        await context.EnsureStorageExists(storageName, cancellationToken);
        await context.EnsureCurrencyExists(currencyId, cancellationToken);
        
        await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
        {
            prevPurchaseDateTime = DateTime.SpecifyKind(prevPurchaseDateTime, DateTimeKind.Unspecified);
            newPurchaseDateTime = DateTime.SpecifyKind(newPurchaseDateTime, DateTimeKind.Unspecified);
        
            var articles = await context.EnsureArticlesExistForUpdate(addRemoveDict.Keys, cancellationToken);
            
            foreach (var (articleId, ls) in addRemoveDict) 
            {
                var article = articles[articleId];
                //Количество которое нам надо. Положительное значит надо добавить на склад, отрицательное значит вычесть со склада.
                int neededCount = ls.Sum(x => x.Value); 
                article.TotalCount += neededCount;
                var totalCount = 0;
            
                //Если надо вычитать со склада, то проверяем доступное количество.
                if (neededCount < 0)
                    totalCount = await context.StorageContents.AsNoTracking()
                        .Where(x => x.ArticleId == articleId && 
                                    x.Count > 0 && x.StorageName == storageName)
                        .SumAsync(x => x.Count, cancellationToken);
                if (-neededCount > totalCount) throw new NotEnoughCountOnStorageException(storageName, articleId, totalCount);

                foreach (var (price, count) in ls.Where(x => x.Value != 0))
                {
                    if (count > 0) //Если надо добавить на склад
                    {
                        var tempPrice = Math.Round(price, 2);
                        var storageItem = await context.StorageContents.FromSqlRaw($"""
                             SELECT * FROM storage_content
                             WHERE article_id = {articleId} AND
                                   currency_id = {currencyId} AND
                                   storage_name = '{storageName}'
                             ORDER BY purchase_datetime - '{prevPurchaseDateTime:yyyy-MM-dd HH:mm:ss.ffffff}' ASC, buy_price - {tempPrice} ASC
                             FOR UPDATE
                             """).FirstOrDefaultAsync(cancellationToken);
                        if (storageItem == null)
                        {
                            storageItem = new StorageContent
                            {
                                StorageName = storageName,
                                ArticleId = articleId,
                                BuyPrice = price,
                                CurrencyId = currencyId,
                                BuyPriceInUsd = CurrencyConverter.ConvertToUsd(price, currencyId),
                                Count = 0
                            };
                            await context.StorageContents.AddAsync(storageItem, cancellationToken);
                        }

                        storageItem.Count += count;
                        storageItem.PurchaseDatetime = newPurchaseDateTime;
                        var movementModel = storageItem.Adapt<StorageMovement>().SetActionType(movementType);
                        movementModel.Count = count;
                        movementModel.WhoMoved = userId;
                        await context.StorageMovements.AddAsync(movementModel, cancellationToken);
                    }
                    else // Списание со склада
                    {
                        int counter = -count;

                        await foreach (var content in context.StorageContents
                                           .FromSqlRaw($"""
                                                            SELECT * FROM storage_content
                                                            WHERE article_id = {articleId}
                                                              AND storage_name = '{storageName}'
                                                              AND count > 0
                                                            ORDER BY purchase_datetime - '{prevPurchaseDateTime:yyyy-MM-dd HH:mm:ss.ffffff}' ASC, count DESC
                                                            FOR UPDATE
                                                        """)
                                           .AsAsyncEnumerable()
                                           .WithCancellation(cancellationToken))
                        {
                            if (content.PurchaseDatetime.Equals(prevPurchaseDateTime))
                                content.PurchaseDatetime = newPurchaseDateTime;
                            
                            var tempMin = Math.Min(content.Count, counter);
                            
                            content.Count -= tempMin;
                            counter -= tempMin;
                            
                            var tempMovement = content.Adapt<StorageMovement>().SetActionType(movementType);
                            tempMovement.Count = -tempMin;
                            tempMovement.WhoMoved = userId;
                            await context.StorageMovements.AddAsync(tempMovement, cancellationToken);
                            
                            if (counter <= 0) break;
                        }

                        if (counter > 0) throw new NotEnoughCountOnStorageException(storageName, articleId, counter);
                    }
                }
            }
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task EditStorageContent(
        Dictionary<int, PatchStorageContentDto> editedFields, 
        string userId,
        CancellationToken cancellationToken = default)
    {
        foreach (var (key, value) in editedFields)
        {
            if (value.Count.IsSet && value.Count < 0)
                throw new StorageContentCountCantBeNegativeException(key);

            if (value.BuyPrice.IsSet && Math.Round(value.BuyPrice, 2) <= 0)
                throw new StorageContentPriceCannotBeNegativeException(key);
        }
        
        var storageContentIds = editedFields.Keys;

        var currencyIds = new HashSet<int>(); 
        var storageNames = new HashSet<string>();
            
        foreach (var value in editedFields.Values)
        {
            if (value.CurrencyId.IsSet) currencyIds.Add(value.CurrencyId);
            if (value.StorageName.IsSet) storageNames.Add(value.StorageName!);
        }
            
        await context.EnsureCurrenciesExist(currencyIds, cancellationToken);
        await context.EnsureStoragesExist(storageNames, cancellationToken);
        
        await context.WithDefaultTransactionSettings("normal-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var storageContents = 
                    await context.EnsureStorageContentsExistForUpdate(storageContentIds, cancellationToken);
                var contentMovements = new List<StorageMovement>();
                var articleIds = storageContents.Values.Select(x => x.ArticleId);
                var articles = await context.EnsureArticlesExistForUpdate(articleIds, cancellationToken);
            
                foreach (var item in editedFields)
                {
                    var content = storageContents[item.Key];
                    var article = articles[content.ArticleId];
                    var newVersion = item.Value;
                    if (newVersion.Count.IsSet)
                    {
                        var diff = newVersion.Count.Value - content.Count;
                        article.TotalCount += diff;
                        var tempMovement = content.Adapt<StorageMovement>()
                            .SetActionType(StorageMovementType.StorageContentEditing);
                        tempMovement.Count = diff;
                        tempMovement.WhoMoved = userId;
                        contentMovements.Add(tempMovement);
                    }
                
                    newVersion.Adapt(content);
                    if (newVersion.BuyPrice.IsSet)
                        content.BuyPriceInUsd = Math.Round(CurrencyConverter.ConvertToUsd(content.BuyPrice, content.CurrencyId), 2);
                }
                await context.AddRangeAsync(contentMovements, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
    }
}