using Core.Extensions;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Main.Persistence.Repositories;

public class SaleRepository(DContext context) : ISaleRepository
{
    public async Task<Sale?> GetSaleForUpdate(string saleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Sales.FromSql($"SELECT * FROM sale where id = {saleId} for update")
            .Include(x => x.Transaction)
            .Include(x => x.SaleContents)
            .ThenInclude(x => x.SaleContentDetails)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContent>> GetSaleContentsForUpdate(string saleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.SaleContents
            .FromSql($"SELECT * FROM sale_content where sale_id = {saleId} for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContentDetail>> GetSaleContentDetailsForUpdate(IEnumerable<int> saleContentIds,
        bool track = true, CancellationToken cancellationToken = default)
    {
        var ids = saleContentIds.Distinct().ToList();
        return await context.SaleContentDetails
            .FromSql($"SELECT * FROM sale_content_details where sale_content_id = ANY({ids}) for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Sale>> GetSales(DateTime rangeStart, DateTime rangeEnd, int page, int viewCount,
        bool track = true,
        string? sortBy = null, string? searchTerm = null, Guid? buyerId = null, int? currencyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Sales.ConfigureTracking(track);
        if (buyerId.HasValue)
            query = query.Where(x => x.BuyerId == buyerId);

        if (currencyId != null) query = query.Where(x => x.CurrencyId == currencyId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
            query = query.Where(x => x.SaleContents
                                         .Any(content => EF.Functions.ToTsVector("russian", content.Article.ArticleName)
                                                             .Matches(
                                                                 EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                                         EF.Functions.ILike(content.Article.NormalizedArticleNumber,
                                                             $"%{normalizedSearchTerm}%")) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")) ||
                                     x.SaleContents.Any(z =>
                                         z.Comment != null && EF.Functions.ILike(z.Comment, $"%{searchTerm}%")));
        }

        var startDate = rangeStart.Date;
        var endDate = rangeEnd.Date.AddDays(1);
        return await query.Where(x => x.CreationDatetime >= startDate && x.CreationDatetime <= endDate)
            .Include(x => x.Transaction)
            .Include(x => x.Buyer)
            .ThenInclude(x => x.UserInfo)
            .Include(x => x.Currency)
            .SortBy(sortBy)
            .Skip(viewCount * page)
            .Take(viewCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContent>> GetSaleContent(string saleId, bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.SaleContents.ConfigureTracking(track)
            .Include(x => x.Article)
            .ThenInclude(x => x.Producer)
            .Where(x => x.SaleId == saleId)
            .ToListAsync(cancellationToken);
    }
}