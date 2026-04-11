using Abstractions.Models.Repository;
using Extensions;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Purchase;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

// ReSharper disable EntityFramework.ClientSideDbFunctionCall

namespace Main.Persistence.Repositories;

public class PurchaseRepository(DContext context) : IPurchaseRepository
{
    public async Task<Purchase?> GetPurchase(
        QueryOptions<Purchase, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.Purchases
            .ApplyOptions(options)
            .Where(x => x.Id == options.Data)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseContent>> GetPurchaseContent(
        QueryOptions<PurchaseContent, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.PurchaseContents
            .ApplyOptions(options)
            .Where(x => x.PurchaseId == options.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Purchase>> GetPurchases(
        QueryOptions<Purchase, GetPurchaseOptionsData> options,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Purchase> query = context.Purchases.ApplyOptions(options);
        if (options.Data.SupplierId != null)
            query = query.Where(x => x.SupplierId == options.Data.SupplierId);

        if (options.Data.CurrencyId != null) 
            query = query.Where(x => x.CurrencyId == options.Data.CurrencyId);

        if (!string.IsNullOrWhiteSpace(options.Data.SearchTerm))
        {
            var searchTerm = options.Data.SearchTerm.Trim();
            var normalizedSearchTerm = searchTerm.ToNormalizedArticleNumber();
            query = query.Where(x => x.PurchaseContents
                                         .Any(content => EF.Functions.ToTsVector("russian", content.Product.Name)
                                                             .Matches(
                                                                 EF.Functions.PlainToTsQuery("russian", searchTerm)) ||
                                                         EF.Functions.ILike(content.Product.NormalizedSku,
                                                             $"%{normalizedSearchTerm}%")) ||
                                     (x.Comment != null && EF.Functions.ILike(x.Comment, $"%{searchTerm}%")) ||
                                     x.PurchaseContents.Any(z =>
                                         z.Comment != null && EF.Functions.ILike(z.Comment, $"%{searchTerm}%")));
        }

        var startDate = options.Data.RangeStart.Date;
        var endDate = options.Data.RangeEnd.Date.AddDays(1);
        var result = await query.Where(x =>
                x.PurchaseDatetime >= startDate.Date && x.PurchaseDatetime <= endDate.Date.AddDays(1))
            .ApplyPaging(options)
            .ToListAsync(cancellationToken);
        return result;
    }
}