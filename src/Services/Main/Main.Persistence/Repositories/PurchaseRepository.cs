using Core.Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Main.Persistence.Repositories;

public class PurchaseRepository(DContext context) : IPurchaseRepository
{
    public async Task<Purchase?> GetPurchaseForUpdate(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Purchases
            .FromSql($"select * from purchase where id = {purchaseId} for update")
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PurchaseContent>> GetPurchaseContentForUpdate(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.PurchaseContents
            .FromSql($"select * from purchase_content where purchase_id = {purchaseId} for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Purchase>> GetPurchases(DateTime rangeStart, DateTime rangeEnd, int page,
        int viewCount, Guid? supplierId, int? currencyId, string? sortBy, string? searchTerm, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Purchases.ConfigureTracking(track);
        if (supplierId != null)
            query = query.Where(x => x.SupplierId == supplierId);

        if (currencyId != null) query = query.Where(x => x.CurrencyId == currencyId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
            query = query.Where(x => x.PurchaseContents
                                         .Any(content => EF.Functions.ToTsVector("russian", content.Article.ArticleName)
                                                             .Matches(
                                                                 EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                                         EF.Functions.ILike(content.Article.NormalizedArticleNumber,
                                                             $"%{normalizedSearchTerm}%")) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")) ||
                                     x.PurchaseContents.Any(z =>
                                         z.Comment != null && EF.Functions.ILike(z.Comment, $"%{searchTerm}%")));
        }

        var startDate = rangeStart.Date;
        var endDate = rangeEnd.Date.AddDays(1);
        var result = await query.Where(x =>
                x.PurchaseDatetime >= startDate.Date && x.PurchaseDatetime <= endDate.Date.AddDays(1))
            .Include(x => x.Transaction)
            .Include(x => x.Supplier)
            .ThenInclude(x => x.UserInfo)
            .Include(x => x.Currency)
            .SortBy(sortBy)
            .Skip(viewCount * page)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<IEnumerable<PurchaseContent>> GetPurchaseContent(string purchaseId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.PurchaseContents.ConfigureTracking(track)
            .Include(x => x.Article)
            .ThenInclude(x => x.Producer)
            .Where(x => x.PurchaseId == purchaseId)
            .ToListAsync(cancellationToken);
    }
}