using Core.TransactionBuilder;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Purchase;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Purchase;

public class Purchase(DContext context) : IPurchase
{
    public async Task<PostGres.Main.Purchase> CreatePurchaseAsync(IEnumerable<(int ArticleId, int Count, decimal Price, string? Comment)> content, int currencyId, 
        string createdUserId, string transactionId, string storageName,
        string supplierId, DateTime purchaseDateTime, string? comment = null, CancellationToken cancellationToken = default)
    {
        return await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
        {
            var contentList = content.ToList();
            if (contentList.Count == 0) throw new PurchaseContentEmptyException();
            if (contentList.Any(x => x.Price == 0 || x.Count == 0)) throw new PurchaseContentPriceOrCountException();
            _ = await context.Currencies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currencyId, cancellationToken: cancellationToken) ?? throw new CurrencyNotFoundException(currencyId);
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == supplierId && x.IsSupplier == true, cancellationToken) ?? throw new SupplierNotFoundException(supplierId);
            _ = await context.AspNetUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == createdUserId, cancellationToken: cancellationToken) ?? throw new UserNotFoundException(createdUserId);
            _ = await context.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken: cancellationToken) ?? throw new TransactionDoesntExistsException(transactionId);
            _ = await context.Storages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name == storageName, cancellationToken) ?? throw new StorageNotFoundException(storageName);
            var articleIds = contentList
                .Select(x => x.ArticleId)
                .Distinct()
                .ToList();
            var foundCount = await context.Articles
                .CountAsync(x => articleIds.Contains(x.Id), cancellationToken);
            if (foundCount != articleIds.Count) throw new ArticleNotFoundException();
            
            var purchaseContents = contentList.Select(x=> new PurchaseContent
            {
                ArticleId = x.ArticleId,
                Price = x.Price,
                Count = x.Count,
                TotalSum = x.Price * x.Count,
                Comment = x.Comment,
            }).ToList();
            var purchaseModel = new PostGres.Main.Purchase
            {
                CurrencyId = currencyId,
                Comment = comment,
                Storage = storageName,
                CreatedUserId = createdUserId,
                SupplierId = supplierId,
                PurchaseDatetime = purchaseDateTime,
                PurchaseContents = purchaseContents,
                TransactionId = transactionId,
            };
            await context.Purchases.AddAsync(purchaseModel, cancellationToken);
            await context.SaveChangesAsync(cancellationToken); 
            return purchaseModel;
        }, cancellationToken);
    }

    public async Task<Dictionary<int, Dictionary<decimal, int>>> EditPurchaseAsync(IEnumerable<EditPurchaseDto> content, string purchaseId, int currencyId, 
        string? comment, string updatedUserId, DateTime purchaseDateTime, CancellationToken cancellationToken = default)
    {
        return await context
            .WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
        {
            var result = new Dictionary<int, Dictionary<decimal, int>>();
            var contentList = content.ToList();
            if (contentList.Count == 0) throw new PurchaseContentEmptyException();
            if (contentList.Any(x => x.Price <= 0 || x.Count <= 0)) throw new PurchaseContentPriceOrCountException();
            _ = await context.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currencyId, cancellationToken) 
                ?? throw new CurrencyNotFoundException(currencyId);
            _ = await context.AspNetUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == updatedUserId, cancellationToken) 
                ?? throw new UserNotFoundException(updatedUserId);
        
            var idsFromClient = contentList.Where(x => x.Id != null).Select(x => x.Id!.Value).ToList();
            var idsFromClientSet = idsFromClient.ToHashSet();
            if (idsFromClient.Count != idsFromClientSet.Count) throw new SamePurchaseContentIdException();
            var purchase = await context.Purchases
                .FromSql($"select * from purchase where id = {purchaseId} for update")
                .FirstOrDefaultAsync(cancellationToken) ?? throw new PurchaseNotFoundException(purchaseId);
            var purchaseContent = await context.PurchaseContents
                .FromSql($"select * from purchase_content where purchase_id = {purchaseId} for update")
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var toDelete = purchaseContent.Values.Where(x => !idsFromClientSet.Contains(x.Id)).ToList();
            
            foreach (var item in idsFromClientSet) result[item] = [];
            foreach (var item in purchaseContent.Values) result[item.ArticleId] = [];
            foreach (var item in toDelete)
                if (!result[item.ArticleId].TryAdd(item.Price, -item.Count))
                    result[item.ArticleId][item.Price] += -item.Count;
            
                
            purchase.Comment = comment?.Trim();
            purchase.CurrencyId = currencyId;
            purchase.PurchaseDatetime = DateTime.SpecifyKind(purchaseDateTime, DateTimeKind.Unspecified);
            purchase.UpdateDatetime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            purchase.UpdatedUserId = updatedUserId;

            foreach (var item in contentList)
            {
                PurchaseContent? samePurchaseContent = null;
                if (item.Id != null && !purchaseContent.TryGetValue(item.Id.Value, out samePurchaseContent))
                    throw new PurchaseContentNotFoundException(item.Id.Value);
                if (item.Id == null)
                {
                    if(!result[item.ArticleId].TryAdd(item.Price, item.Count))
                        result[item.ArticleId][item.Price] += item.Count;
                    samePurchaseContent = item.Adapt<PurchaseContent>();
                    await context.PurchaseContents.AddAsync(samePurchaseContent, cancellationToken);
                }
                else
                { 
                    if (item.ArticleId != samePurchaseContent!.ArticleId)
                        throw new ArticleDoesntMatchContentException(item.ArticleId);
                    samePurchaseContent = item.Adapt(samePurchaseContent);
                    int delta = item.Count - samePurchaseContent.Count;
                    if (delta == 0) continue;
                    if (!result[item.ArticleId].TryAdd(item.Price, delta))
                        result[item.ArticleId][item.Price] += delta;
                }
                
            }
            
            foreach (var kv in result.ToList())
            {
                foreach (var price in kv.Value.Where(x => x.Value == 0).Select(x => x.Key).ToList())
                    kv.Value.Remove(price);

                if (kv.Value.Count == 0) result.Remove(kv.Key);
            }
            
            context.PurchaseContents.RemoveRange(toDelete);
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }, cancellationToken: cancellationToken);
    }

    public async Task DeletePurchase(string purchaseId, CancellationToken cancellationToken = default)
    {
        await context.WithDefaultTransactionSettings("normal")
            .ExecuteWithTransaction(async () =>
        {
            var purchase = await context.Purchases
                .FromSql($"select * from purchase where id = {purchaseId} for update")
                .FirstOrDefaultAsync(cancellationToken) ?? throw new PurchaseNotFoundException(purchaseId);
            context.Purchases.Remove(purchase);
            await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken: cancellationToken);
    }
}