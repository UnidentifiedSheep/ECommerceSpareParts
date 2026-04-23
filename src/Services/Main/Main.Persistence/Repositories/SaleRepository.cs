using Abstractions.Models.Repository;
using Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.RepositoryOptionsData;
using Main.Entities;
using Main.Entities.Sale;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Main.Persistence.Repositories;

public class SaleRepository(DContext context) : ISaleRepository
{
    public async Task<Sale?> GetSaleForUpdate(
        string saleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.Sales.FromSql($"SELECT * FROM sale where id = {saleId} for update")
            .Include(x => x.Transaction)
            .Include(x => x.SaleContents)
            .ThenInclude(x => x.SaleContentDetails)
            .ConfigureTracking(track)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContent>> GetSaleContentsForUpdate(
        string saleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.SaleContents
            .FromSql($"SELECT * FROM sale_content where sale_id = {saleId} for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContentDetail>> GetSaleContentDetailsForUpdate(
        IEnumerable<int> saleContentIds,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        var ids = saleContentIds.Distinct().ToList();
        return await context.SaleContentDetails
            .FromSql($"SELECT * FROM sale_content_details where sale_content_id = ANY({ids}) for update")
            .ConfigureTracking(track)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Sale>> GetSales(
        QueryOptions<Sale, GetSalesOptionsData> options,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> query = context.Sales;
        if (options.Data.BuyerId.HasValue)
            query = query.Where(x => x.BuyerId == options.Data.BuyerId.Value);

        if (options.Data.CurrencyId != null) 
            query = query.Where(x => x.CurrencyId == options.Data.CurrencyId);

        if (!string.IsNullOrWhiteSpace(options.Data.SearchTerm))
        {
            var searchTerm = options.Data.SearchTerm.Trim();
            var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
            query = query.Where(x => x.SaleContents
                                         .Any(content => EF.Functions.ToTsVector("russian", content.Product.Name)
                                                             .Matches(
                                                                 EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                                         EF.Functions.ILike(content.Product.NormalizedSku,
                                                             $"%{normalizedSearchTerm}%")) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")) ||
                                     x.SaleContents.Any(z =>
                                         z.Comment != null && EF.Functions.ILike(z.Comment, $"%{searchTerm}%")));
        }

        var startDate = options.Data.RangeStart.Date;
        var endDate = options.Data.RangeEnd.Date.AddDays(1);
        return await query
            .Where(x => x.SaleDatetime >= startDate && x.SaleDatetime <= endDate)
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SaleContent>> GetSaleContent(
        Guid saleId,
        bool track = true,
        CancellationToken cancellationToken = default)
    {
        return await context.SaleContents.ConfigureTracking(track)
            .Include(x => x.Product)
            .ThenInclude(x => x.Producer)
            .Where(x => x.SaleId == saleId)
            .ToListAsync(cancellationToken);
    }
}