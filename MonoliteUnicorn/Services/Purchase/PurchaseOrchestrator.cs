using Core.TransactionBuilder;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Purchase;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Purchase;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using MonoliteUnicorn.Services.Inventory;
using MonoliteUnicorn.Services.Prices.Price;

namespace MonoliteUnicorn.Services.Purchase;

public class PurchaseOrchestrator(IServiceProvider serviceProvider) : IPurchaseOrchestrator
{
    public async Task CreateFullPurchase(string createdUserId, string supplierId, int currencyId, string storageName, DateTime purchaseDate, 
        IEnumerable<NewPurchaseContentDto> purchaseContent, string? comment, decimal? payedSum, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchase>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();
        var pricesService = scope.ServiceProvider.GetRequiredService<IPrice>();
        
        await context.WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var purchaseContentList = purchaseContent.ToList();
                var contentAsTuple = purchaseContentList.Select(x => (x.ArticleId, x.Count, x.Price, x.Comment));
                var totalSum = purchaseContentList.Sum(x => x.Count * x.Price);
                var transaction = await balanceService.CreateTransactionAsync(supplierId, "SYSTEM", totalSum,
                    TransactionStatus.Purchase, currencyId, createdUserId, purchaseDate, cancellationToken);
                await purchaseService.CreatePurchaseAsync(contentAsTuple, currencyId, createdUserId, transaction.Id,
                    storageName, supplierId, purchaseDate, comment, cancellationToken);
                var inventoryItems = await inventoryService.AddContentToStorage(
                    purchaseContentList.Select(x => (x.ArticleId, x.Count, x.Price, currencyId)), storageName,
                    createdUserId, StorageMovementType.Purchase, cancellationToken);
                if (payedSum is > 0)
                    await balanceService.CreateTransactionAsync("SYSTEM", supplierId, payedSum.Value,
                        TransactionStatus.Normal, currencyId, createdUserId, purchaseDate.AddMicroseconds(1),
                        cancellationToken);
                await pricesService.RecalculateUsablePrice(inventoryItems.Select(x => (x.ArticleId, x.BuyPriceInUsd)),
                    cancellationToken);
            }, cancellationToken);
    }

    public async Task EditPurchase(IEnumerable<EditPurchaseDto> content, string purchaseId, int currencyId, string? comment, 
        string updatedUserId, DateTime purchaseDateTime, CancellationToken cancellationToken = default)
    {
        purchaseDateTime = DateTime.SpecifyKind(purchaseDateTime, DateTimeKind.Unspecified);
        var contentList = content.ToList();
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchase>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();

        await context
            .WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var purchase = await context.Purchases.AsNoTracking()
                                   .Include(x => x.Transaction)
                                   .FirstOrDefaultAsync(x => x.Id == purchaseId, cancellationToken) ??
                               throw new PurchaseNotFoundException(purchaseId);
                var newTotalSum = contentList.Sum(x => x.Count * x.Price);
                var editArticleCounts = await purchaseService.EditPurchaseAsync(contentList, purchaseId, currencyId,
                    comment, updatedUserId, purchaseDateTime, cancellationToken);
                await balanceService.EditTransaction(purchase.TransactionId, currencyId, newTotalSum,
                    TransactionStatus.Purchase, purchaseDateTime, cancellationToken);
                await inventoryService.AddOrRemoveContentFromStorage(editArticleCounts, purchase.CurrencyId,
                    purchase.Storage,
                    purchase.PurchaseDatetime, purchaseDateTime, updatedUserId, StorageMovementType.PurchaseEditing,
                    cancellationToken);
            }, cancellationToken);
    }

    public async Task DeletePurchase(string purchaseId, string userId, CancellationToken cancellationToken = default)
    {
        var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DContext>();
        var purchaseService = scope.ServiceProvider.GetRequiredService<IPurchase>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventory>();

        await context.WithDefaultTransactionSettings("orchestrator-with-isolation")
            .ExecuteWithTransaction(async () =>
            {
                var purchase = await context.Purchases
                    .FromSql($"select * from purchase where id = {purchaseId} for update")
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new PurchaseNotFoundException(purchaseId);
                var transactionId = purchase.TransactionId;
                var purchaseContent = await context.PurchaseContents
                    .FromSql($"select * from purchase_content where purchase_id = {purchaseId} for update")
                    .ToListAsync(cancellationToken);
                var content = purchaseContent
                    .GroupBy(pc => pc.ArticleId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(pc => pc.Price)
                            .ToDictionary(
                                gg => gg.Key,
                                gg => gg.Sum(pc => -pc.Count)
                            )
                    );

                await inventoryService.AddOrRemoveContentFromStorage(content, purchase.CurrencyId, purchase.Storage,
                    purchase.PurchaseDatetime, purchase.PurchaseDatetime, userId, StorageMovementType.PurchaseDeletion,
                    cancellationToken);
                await purchaseService.DeletePurchase(purchaseId, cancellationToken);
                await balanceService.DeleteTransaction(transactionId, userId, cancellationToken);
            }, cancellationToken);

    }
}