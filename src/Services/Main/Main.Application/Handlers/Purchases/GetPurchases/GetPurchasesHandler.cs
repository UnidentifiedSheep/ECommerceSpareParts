using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Projections;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetPurchases;

public record GetPurchasesQuery(
    RangeModel<DateTime> DateRange,
    Pagination Pagination,
    IEnumerable<Guid> SupplierIds,
    IEnumerable<int> CurrencyIds,
    IEnumerable<int> ProductIds,
    string? SortBy,
    string? SearchTerm
) : IQuery<GetPurchasesResult>;

public record GetPurchasesResult(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesHandler(
    IReadRepository<Purchase, Guid> repository
) : IQueryHandler<GetPurchasesQuery, GetPurchasesResult>
{
    public async Task<GetPurchasesResult> Handle(
        GetPurchasesQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.DateRange.Min.HasValue)
            query = query.Where(x => x.PurchaseDatetime >= request.DateRange.Min.Value);

        if (request.DateRange.Max.HasValue)
            query = query.Where(x => x.PurchaseDatetime <= request.DateRange.Max.Value);

        if (request.SupplierIds.Any()) query = query.Where(x => request.SupplierIds.Contains(x.SupplierId));

        if (request.CurrencyIds.Any()) query = query.Where(x => request.CurrencyIds.Contains(x.CurrencyId));

        if (request.ProductIds.Any())
            query = query.Where(x => x.Contents.Any(z => request.ProductIds.Contains(z.ProductId)));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = ApplySearchTerm(query, request.SearchTerm);

        var purchases = await query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(PurchaseProjections.ToPurchaseDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetPurchasesResult(purchases);
    }

    private static IQueryable<Purchase> ApplySearchTerm(
        IQueryable<Purchase> query,
        string searchTerm)
    {
        var term = searchTerm.Trim();
        var pattern = $"%{term}%";

        return query.Where(x =>
            EF.Functions.ILike(x.Comment!, pattern) ||
            x.Contents.Any(z =>
                EF.Functions.ILike(z.Comment!, pattern) ||
                EF.Functions.ILike(z.Product.Name.Value, pattern) ||
                EF.Functions.ILike(z.Product.Sku.NormalizedValue, pattern)));
    }
}