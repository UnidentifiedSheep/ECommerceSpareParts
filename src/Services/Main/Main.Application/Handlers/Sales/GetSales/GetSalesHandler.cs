using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Projections;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(
    RangeModel<DateTime> DateRange,
    Pagination Pagination,
    IEnumerable<Guid> BuyerIds,
    IEnumerable<int> CurrencyIds,
    IEnumerable<int> ProductIds,
    string? SortBy,
    string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IReadOnlyList<SaleDto> Sales);

public class GetSalesHandler(
    IReadRepository<Sale, Guid> repository
) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.DateRange.Min.HasValue)
            query = query.Where(x => x.SaleDatetime >= request.DateRange.Min.Value);

        if (request.DateRange.Max.HasValue)
            query = query.Where(x => x.SaleDatetime <= request.DateRange.Max.Value);

        if (request.CurrencyIds.Any())
            query = query.Where(x => request.CurrencyIds.Contains(x.CurrencyId));

        if (request.BuyerIds.Any())
            query = query.Where(x => request.BuyerIds.Contains(x.BuyerId));

        if (request.ProductIds.Any())
            query = query.Where(x => x.Contents.Any(z => request.ProductIds.Contains(z.ProductId)));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = ApplySearchTerm(query, request.SearchTerm);

        var result = await query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(SaleProjections.ToSaleDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetSalesResult(result);
    }

    private static IQueryable<Sale> ApplySearchTerm(
        IQueryable<Sale> query,
        string searchTerm)
    {
        var term = searchTerm.Trim();
        var pattern = $"%{term}%";

        return query.Where(x =>
            EF.Functions.ILike(x.Comment!, pattern) ||
            EF.Functions.ILike(x.Buyer.UserName!, pattern) ||
            x.Contents.Any(z =>
                EF.Functions.ILike(z.Comment!, pattern) ||
                EF.Functions.ILike(z.Product.Name.Value, pattern) ||
                EF.Functions.ILike(z.Product.Sku.NormalizedValue, pattern)));
    }
}
