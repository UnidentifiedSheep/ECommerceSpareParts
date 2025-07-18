using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Sales;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Sale;

public class Sale(DContext context) : ISale
{
    public async Task<PostGres.Main.Sale> CreateSale(IEnumerable<NewSaleContentDto> sellContent, IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues, 
        int currencyId, string buyerId, string createdUserId, string transactionId, string mainStorage,
        DateTime saleDateTime, string? comment, CancellationToken cancellationToken = default) 
    {
        var saleContentList = sellContent
            .OrderByDescending(x => x.ArticleId)
            .ThenByDescending(x => x.Count)
            .ThenByDescending(x => x.PriceWithDiscount)
            .ToList();

        if (saleContentList.Count == 0)
            throw new SalesContentEmptyException();
        if (saleContentList.Any(x => Math.Round(x.PriceWithDiscount, 2) <= 0))
            throw new SaleContentPriceOrCountException();
                
        var articleNeededCounts = new Dictionary<int, int>();
        var saleContents = new List<SaleContent>();
        var articleIds = new HashSet<int>();
        foreach (var item in saleContentList)
            articleIds.Add(item.ArticleId);


        var detailGroups = GetDetailsGroup(storageContentValues);

        await context.EnsureArticlesExist(articleIds, cancellationToken);
        await context.EnsureCurrencyExists(currencyId, cancellationToken);
        await context.EnsureUserExists([buyerId, createdUserId], cancellationToken);
        await context.EnsureTransactionExists(transactionId, cancellationToken);
        await context.EnsureStorageExists(mainStorage, cancellationToken);
        
        return await context
            .WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () => 
            { 
                foreach (var item in saleContentList)
                {
                    if (item.Price <= 0 || item.PriceWithDiscount <= 0 || item.Count <= 0 || item.Price < item.PriceWithDiscount)
                        throw new SaleContentPriceOrCountException();

                    var saleContent = item.Adapt<SaleContent>();
                    saleContents.Add(saleContent);

                    if (!articleNeededCounts.TryAdd(item.ArticleId, item.Count))
                        articleNeededCounts[item.ArticleId] += item.Count;

                    if (!detailGroups.TryGetValue(item.ArticleId, out var queue))
                        throw new ArgumentException($"Нет деталей по артикулу {item.ArticleId}");

                    int counter = item.Count;
                    while (counter > 0 && queue.Count > 0)
                    {
                        var detail = queue.Peek();
                        if (detail.Count <= counter)
                        {
                            counter -= detail.Count;
                            saleContent.SaleContentDetails.Add(detail);
                            queue.Dequeue();
                        }
                        else
                        {
                            var partial = detail.Adapt<SaleContentDetail>();
                            partial.Count = counter;
                            detail.Count -= counter;
                            counter = 0;
                            saleContent.SaleContentDetails.Add(partial);
                        }
                    }

                    if (counter > 0)
                        throw new ArgumentException($"Недостаточно деталей для артикула {item.ArticleId}");
                }
                
                if (detailGroups.Any(x => x.Value.Count > 0))
                    throw new ArgumentException("Несовпадение количества в деталях и продажах");
                

                var saleModel = new PostGres.Main.Sale
                {
                    TransactionId = transactionId,
                    SaleDatetime = saleDateTime,
                    BuyerId = buyerId,
                    CreatedUserId = createdUserId,
                    Comment = comment,
                    SaleContents = saleContents,
                    CurrencyId = currencyId,
                    MainStorageName = mainStorage,
                };

                await context.Sales.AddAsync(saleModel, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return saleModel; 
            }, cancellationToken);
    }

    public async Task<PostGres.Main.Sale> DeleteSale(string saleId, string whoDeletedUserId, CancellationToken cancellationToken = default)
    {
        await context.EnsureUserExists(whoDeletedUserId, cancellationToken);
        return await context
            .WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () => 
            {
                var sale = await context.Sales.FromSql($"SELECT * FROM sale where id = {saleId} for update")
                    .Include(x => x.SaleContents)
                    .ThenInclude(x => x.SaleContentDetails)
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new SaleNotFoundException(saleId);
                context.Sales.Remove(sale);
                await context.SaveChangesAsync(cancellationToken);
                return sale; 
            }, cancellationToken);
    }

    public async Task EditSale(IEnumerable<EditSaleContentDto> editedContent,
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues,
        Dictionary<int, List<SaleContentDetail>> movedToStorage,
        string saleId, int currencyId, string updatedUserId, 
        DateTime saleDateTime, string? comment,
        CancellationToken cancellationToken = default)
    {
        await context
            .WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () => 
            {
                _ = await context.Currencies.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == currencyId, cancellationToken) ?? throw new CurrencyNotFoundException(currencyId);
                _ = await context.AspNetUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == updatedUserId, cancellationToken) ?? throw new UserNotFoundException(updatedUserId);
            
                saleDateTime = DateTime.SpecifyKind(saleDateTime, DateTimeKind.Unspecified);
            
                var sale = await context.Sales.FromSql($"SELECT * FROM sale where id = {saleId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new SaleNotFoundException(saleId);
                sale.Comment = comment;
                sale.SaleDatetime = saleDateTime;
                sale.UpdatedUserId = updatedUserId;
                sale.UpdateDatetime = DateTime.Now;
                
                var saleContents = await context.SaleContents
                    .FromSql($"SELECT * FROM sale_content where sale_id = {saleId} for update")
                    .ToDictionaryAsync(x => x.Id, cancellationToken);
                var saleContentDetails = await context.SaleContentDetails
                    .FromSql($"SELECT * FROM sale_content_details where sale_content_id = ANY({saleContents.Keys}) for update")
                    .ToDictionaryAsync(x => x.Id, cancellationToken);
                var detailGroups = GetDetailsGroup(storageContentValues);
                var deletedContentIds = new HashSet<int>(saleContents.Keys);
                foreach (var item in editedContent)
                {
                    if (item.Id != null)
                    {
                        deletedContentIds.Remove(item.Id.Value);
                        if(!saleContents.TryGetValue(item.Id.Value, out var saleContent))
                            throw new SaleContentNotFoundException(item.Id.Value);
                        
                        saleContent.Discount = (item.Price - item.PriceWithDiscount) / item.Price * 100;
                        saleContent.Price = item.PriceWithDiscount;
                        saleContent.TotalSum = item.PriceWithDiscount * item.Count;
                        
                        if (saleContent.Count < item.Count)
                        {
                            if (!detailGroups.TryGetValue(item.ArticleId, out var queue))
                                throw new ArgumentException($"Нет деталей по артикулу {item.ArticleId}");

                            int counter = item.Count - saleContent.Count;
                            while (counter > 0 && queue.Count > 0)
                            {
                                var detail = queue.Peek();
                                if (detail.Count <= counter)
                                {
                                    counter -= detail.Count;
                                    saleContent.SaleContentDetails.Add(detail);
                                    queue.Dequeue();
                                }
                                else
                                {
                                    var partial = detail.Adapt<SaleContentDetail>();
                                    partial.Count = counter;
                                    detail.Count -= counter;
                                    counter = 0;
                                    saleContent.SaleContentDetails.Add(partial);
                                }
                            }

                            if (counter > 0)
                                throw new ArgumentException($"Недостаточно деталей для артикула {item.ArticleId}");
                        }
                        else if(saleContent.Count > item.Count)
                        {
                            foreach (var tempDetail in movedToStorage[item.Id.Value])
                            {
                                var realDetail = saleContentDetails[tempDetail.Id];
                                if (realDetail.Count < tempDetail.Count)
                                    throw new ArgumentException("В продаже нет достаточного количества элементов для возврата на склад");
                                realDetail.Count -= tempDetail.Count;
                                if (realDetail.Count == 0)
                                    context.Remove(realDetail);
                            }
                        }
                        
                        saleContent.Count = item.Count;
                    }
                    else
                    {
                        var saleContent = item.Adapt<SaleContent>();
                        sale.SaleContents.Add(saleContent);
                        if (!detailGroups.TryGetValue(item.ArticleId, out var queue))
                            throw new ArgumentException($"Нет деталей по артикулу {item.ArticleId}");
                        
                        int counter = item.Count;
                        while (counter > 0 && queue.Count > 0)
                        {
                            var detail = queue.Peek();
                            if (detail.Count <= counter)
                            {
                                counter -= detail.Count;
                                saleContent.SaleContentDetails.Add(detail);
                                queue.Dequeue();
                            }
                            else
                            {
                                var partial = detail.Adapt<SaleContentDetail>();
                                partial.Count = counter;
                                detail.Count -= counter;
                                counter = 0;
                                saleContent.SaleContentDetails.Add(partial);
                            }
                        }
                        
                        if (counter > 0)
                            throw new ArgumentException($"Недостаточно деталей для артикула {item.ArticleId}");
                    }
                }
                
                if (detailGroups.Any(x => x.Value.Count > 0))
                    throw new ArgumentException("Несовпадение количества в деталях и продажах");
                var deletedContents = saleContents
                    .Where(kvp => deletedContentIds.Contains(kvp.Key))
                    .Select(x => x.Value)
                    .ToList();
                context.RemoveRange(deletedContents);
                await context.SaveChangesAsync(cancellationToken); 
            }, cancellationToken);
    }

    private Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues)
    {
        return storageContentValues.Select(x => {
                var taken = x.Prev.Count - x.NewValue.Count;
                if (taken <= 0 || taken > x.Prev.Count)
                    throw new ArgumentException("Некорректное taken количество");
                if (x.Prev.Id != x.NewValue.Id)
                    throw new ArgumentException("Не совпадает Id в старом и новом значении");
                var detail = x.NewValue.Adapt<SaleContentDetail>();
                detail.Count = taken;

                return (x.Prev.ArticleId, Detail: detail);
            })
            .GroupBy(x => x.ArticleId)
            .ToDictionary(
                g => g.Key,
                g => new Queue<SaleContentDetail>(
                    g.Select(x => x.Detail)
                        .OrderByDescending(x => x.Count)
                        .ThenByDescending(x => x.BuyPrice)
                )
            );
    }

}