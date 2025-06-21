using Core.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Prices.PriceGenerator;

namespace MonoliteUnicorn.Services.Inventory;

public class Inventory(DContext context) : IInventory
{
    public async Task<IEnumerable<StorageContent>> AddContentToStorage(IEnumerable<(int ArticleId, int Count, decimal Price)> content, int currencyId, 
        string storageName, string userId, StorageContentStatus status, 
        StorageMovementType movementType, CancellationToken cancellationToken = default)
    {
        _ = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == currencyId, cancellationToken) ?? throw new CurrencyNotFoundException(currencyId);
        _ = await context.Storages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == storageName, cancellationToken) ?? throw new StorageNotFoundException(storageName);
        _ = await context.AspNetUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) ?? throw new UserNotFoundException(userId);
        
        bool isLocalDbTransaction = context.Database.CurrentTransaction == null;
        
        try
        {
            var dbTransaction = context.Database.CurrentTransaction ?? await context.Database.BeginTransactionAsync(cancellationToken);
            
            var contentList = content.ToList();
            var storageContent = contentList.Select(x => new StorageContent
            {
                StorageName = storageName,
                ArticleId = x.ArticleId,
                BuyPrice = x.Price,
                CurrencyId = currencyId,
                Status = status.ToString(),
                BuyPriceInUsd = PriceGenerator.ConvertToNeededCurrency(x.Price, currencyId, Global.UsdId),
                Count = x.Count
            }).ToList();
            var storageMovement = contentList.Select(x =>
            {
                var k = x.Adapt<StorageMovement>().SetActionType(movementType);
                k.WhoMoved = userId;
                return k;
            });
            await context.StorageMovements.AddRangeAsync(storageMovement, cancellationToken);
            await context.StorageContents.AddRangeAsync(storageContent, cancellationToken);
            foreach (var item in storageContent)
            {
                var article = await context.Articles
                    .FromSql($"select * from articles where id = {item.ArticleId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new ArticleNotFoundException(item.ArticleId);
                article.TotalCount += item.Count;
            }
            await context.SaveChangesAsync(cancellationToken);
            if (!isLocalDbTransaction) return storageContent;
            await dbTransaction.CommitAsync(cancellationToken);
            await dbTransaction.DisposeAsync();
            return storageContent;
        }
        catch (Exception)
        {
            if (!isLocalDbTransaction || context.Database.CurrentTransaction == null) throw;
            await context.Database.CurrentTransaction!.RollbackAsync(cancellationToken);
            await context.Database.CurrentTransaction.DisposeAsync();
            throw;
        }
    }

    public async Task AddContentToStorage(IEnumerable<(SaleContentDetail, int)> contentDetails, StorageMovementType movementType, string userId,
        CancellationToken cancellationToken = default)
    {
        await context.WithTransactionAsync(async () =>
        {
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) ?? throw new UserNotFoundException(userId);
            var contentDetailsList = contentDetails.ToList();
            var articleIds = contentDetailsList
                .Select(x => x.Item2)
                .ToHashSet();
            var articles = await context.Articles
                .FromSql($"SELECT * from articles where id = ANY ({articleIds.ToArray()}) for update")
                .ToDictionaryAsync(x => x.Id,cancellationToken);
            var notFoundArticles = articleIds.Except(articles.Select(x => x.Key)).ToList();
            if (notFoundArticles.Count != 0)
                throw new ArticleNotFoundException(notFoundArticles);
            foreach (var (contentDetail, articleId) in contentDetailsList)
            {
                var article = articles[articleId];
                var storageContent = await context.StorageContents.FromSql($"""
                                                                            SELECT * 
                                                                            FROM storage_content 
                                                                            where id = {contentDetail.StorageContentId} and 
                                                                                  article_id = {articleId} and 
                                                                                  storage_name = {contentDetail.Storage} and 
                                                                                  status = {nameof(StorageContentStatus.Ok)}
                                                                            for update
                                                                            """).FirstOrDefaultAsync(cancellationToken);
                if (storageContent != null)
                    storageContent.Count += contentDetail.Count;
                else
                {
                    storageContent = contentDetail.Adapt<StorageContent>();
                    storageContent.Status = nameof(StorageContentStatus.Ok);
                    storageContent.BuyPriceInUsd = PriceGenerator.ConvertToUsd(storageContent.BuyPrice, storageContent.CurrencyId);
                    storageContent.ArticleId = articleId;
                    await context.AddAsync(storageContent, cancellationToken);
                }
                var tempMovement = storageContent.Adapt<StorageMovement>().SetActionType(movementType);
                tempMovement.Count = contentDetail.Count;
                tempMovement.WhoMoved = userId;
                await context.StorageMovements.AddAsync(tempMovement, cancellationToken);
                
                article.TotalCount += contentDetail.Count;
            }
            
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task DeleteContentFromStorage(int contentId, string userId, StorageMovementType movementType,
        CancellationToken cancellationToken = default)
    {
        await context.WithTransactionAsync(async () =>
        {
            var user = await context.AspNetUsers.AnyAsync(x => x.Id == userId, cancellationToken);
            if (!user) 
                throw new UserNotFoundException(userId);
            var content = await context.StorageContents
                .FromSql($"select * from storage_content where id = {contentId} for update")
                .FirstOrDefaultAsync(cancellationToken) ?? throw new StorageContentNotFoundException(contentId);
            
            if (content.Status != nameof(StorageContentStatus.Ok))
                throw new BadStorageContentStatusException(content.Status);

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
        string userId, string? storageName, bool takeFromOtherStorages, 
        StorageMovementType movementType, CancellationToken cancellationToken = default)
    {
        return await context.WithTransactionAsync(async () =>
        {
            if (!takeFromOtherStorages && string.IsNullOrWhiteSpace(storageName)) 
                throw new StorageIsUnknownException();
            _ = await context.AspNetUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) ?? throw new UserNotFoundException(userId);
           var result = new List<PrevAndNewValue<StorageContent>>();
            var mergedContent = new Dictionary<int, int>();

            foreach (var ck in content)
            {
                if (ck.Count < 0) throw new ArgumentException("Количество для удаления со склада не может быть отрицательным");
                if (!mergedContent.TryAdd(ck.ArticleId, ck.Count))
                    mergedContent[ck.ArticleId] += ck.Count;
            }
            foreach (var (articleId, count) in mergedContent)
            {
                List<StorageContent> storageContents = [];
                
                int availableCount = 0;
                var article = await context.Articles
                    .FromSql($"select * from articles where id = {articleId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new ArticleNotFoundException(articleId);
                if (!string.IsNullOrWhiteSpace(storageName))
                {
                    await foreach (var item in context.StorageContents
                                       .FromSql($"""
                                                 select * from storage_content 
                                                 where article_id = {articleId} 
                                                   and status = {nameof(StorageContentStatus.Ok)} 
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
                                                    AND status = {nameof(StorageContentStatus.Ok)}
                                                    AND storage_name != {storageName}
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
                article.TotalCount -= count;
            }
            
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }, cancellationToken);
    }

    public async Task AddOrRemoveContentFromStorage(Dictionary<int, Dictionary<decimal, int>> addRemoveDict, int currencyId, string storageName,
        DateTime prevPurchaseDateTime, DateTime newPurchaseDateTime, string userId,
        StorageMovementType movementType, CancellationToken cancellationToken = default)
    {
        await context.WithTransactionAsync(async () =>
        {
            if (addRemoveDict.Count == 0) return;
            prevPurchaseDateTime = DateTime.SpecifyKind(prevPurchaseDateTime, DateTimeKind.Unspecified);
            newPurchaseDateTime = DateTime.SpecifyKind(newPurchaseDateTime, DateTimeKind.Unspecified);
        
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken) ?? throw new UserNotFoundException(userId);
        
            foreach (var item in addRemoveDict) 
            {
                var articleId = item.Key;
                var ls = item.Value;
                var article = await context.Articles
                    .FromSql($"select * from articles where id = {articleId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new ArticleNotFoundException(articleId);
                //Количество которое нам надо. Положительное значит надо добавить на склад, отрицательное значит вычесть со склада.
                int neededCount = ls.Sum(x => x.Value); 
                article.TotalCount += neededCount;
                var totalCount = 0;
            
                //Если надо вычитать со склада, то проверяем доступное количество.
                if (neededCount < 0)
                    totalCount = await context.StorageContents.AsNoTracking()
                        .Where(x => x.ArticleId == articleId && 
                                    x.Count > 0 && x.StorageName == storageName &&
                                    x.Status == nameof(StorageContentStatus.Ok))
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
                                   status = '{nameof(StorageContentStatus.Ok)}' AND
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
                                Status = nameof(StorageContentStatus.Ok),
                                BuyPriceInUsd = PriceGenerator.ConvertToNeededCurrency(price, currencyId, Global.UsdId),
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
                                                              AND status = '{nameof(StorageContentStatus.Ok)}'
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
        await context.WithTransactionAsync(async () =>
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
            var currencies = await context.Currencies
                .AsNoTracking()
                .Where(x => currencyIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            
            var missedCurrencies = currencyIds.Except(currencies).ToList();
            if (missedCurrencies.Count > 0) throw new CurrencyNotFoundException(missedCurrencies);
            
            var storages = await context.Storages
                .AsNoTracking()
                .Where(x => storageNames.Contains(x.Name))
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);
            var missedStorages = storageNames.Except(storages).ToList();
            if (missedStorages.Count > 0) throw new StorageNotFoundException(missedStorages);
            
            var storageContents = await context.StorageContents
                .FromSql($"Select * from storage_content where id = Any({storageContentIds.ToArray()}) for update")
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var missingIds = storageContentIds.Except(storageContents.Keys).ToList();
            if (missingIds.Count != 0) throw new StorageContentNotFoundException(missingIds);

            var articleIds = storageContents.Values.Select(x => x.ArticleId);
            var articles = await context.Articles
                .FromSql($"Select * from articles where id = ANY({articleIds.ToArray()}) for update")
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            
            foreach (var item in editedFields)
            {
                var content = storageContents[item.Key];
                var article = articles[content.ArticleId];
                var newVersion = item.Value;
                if (newVersion.Count.IsSet)
                {
                    var diff = newVersion.Count.Value - content.Count;
                    article.TotalCount += diff;
                    var tempMovement = content.Adapt<StorageMovement>().SetActionType(StorageMovementType.StorageContentEditing);
                    tempMovement.Count = diff;
                    tempMovement.WhoMoved = userId;
                    await context.AddAsync(tempMovement, cancellationToken);
                }
                
                newVersion.Adapt(content);
                if (newVersion.BuyPrice.IsSet)
                    content.BuyPriceInUsd = PriceGenerator.ConvertToUsd(content.BuyPrice, content.CurrencyId);

            }
            
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}