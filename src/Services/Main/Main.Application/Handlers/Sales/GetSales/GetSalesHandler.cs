using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Main.Enums;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(
    RangeModel<DateTime> DateRange,
    Pagination Pagination,
    IEnumerable<Guid> OrganizationIds,
    IEnumerable<int> CurrencyIds,
    IEnumerable<int> ProductIds,
    IEnumerable<SaleState> States,
    string[] SortBy,
    string? SearchTerm
) : IQuery<GetSalesResult>;

public record GetSalesResult(IReadOnlyList<SaleDto> Sales);

public class GetSalesHandler(
    IReadRepository<Sale, Guid> repository,
    IProjectionProvider<Sale, SaleDto> projection
) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.States.Any()) query = query.Where(x => request.States.Contains(x.State));

        if (request.DateRange.Min.HasValue)
            query = query.Where(x => x.SaleDatetime >= request.DateRange.Min.Value);

        if (request.DateRange.Max.HasValue)
            query = query.Where(x => x.SaleDatetime <= request.DateRange.Max.Value);

        if (request.CurrencyIds.Any()) query = query.Where(x => request.CurrencyIds.Contains(x.CurrencyId));

        if (request.OrganizationIds.Any()) query = query.Where(x => request.OrganizationIds.Contains(x.OrganizationId));

        if (request.ProductIds.Any())
            query = query.Where(x => x.Contents.Any(z => request.ProductIds.Contains(z.ProductId)));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = ApplySearchTerm(query, request.SearchTerm);

        var result = await query
            .SortBy(request.SortBy)
            .Project(projection)
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
            EF.Functions.ILike(x.User.UserName!, pattern) ||
            x.Contents.Any(z =>
                EF.Functions.ILike(z.Comment!, pattern) ||
                EF.Functions.ILike(z.Product.Name.Value, pattern) ||
                EF.Functions.ILike(z.Product.Sku.NormalizedValue, pattern)));
    }
}
