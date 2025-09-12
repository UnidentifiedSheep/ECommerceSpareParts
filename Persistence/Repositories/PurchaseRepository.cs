using Core.Entities;
using Core.Extensions;
using Core.Interfaces.DbRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Extensions;

// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Persistence.Repositories;

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
        int viewCount,
        string? supplierId, int? currencyId, string? sortBy, string? searchTerm, bool track = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.Purchases.ConfigureTracking(track);
        if (!string.IsNullOrWhiteSpace(supplierId))
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
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")));
        }

        var startDate = DateTime.SpecifyKind(rangeStart.Date, DateTimeKind.Unspecified);
        var endDate = DateTime.SpecifyKind(rangeEnd.Date, DateTimeKind.Unspecified).AddDays(1);
        var result = await query.Where(x => x.CreationDatetime >= startDate && x.CreationDatetime <= endDate)
            .Include(x => x.Transaction)
            .Include(x => x.Supplier)
            .SortBy(sortBy)
            .Skip(viewCount * page)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
        return result;
    }
}