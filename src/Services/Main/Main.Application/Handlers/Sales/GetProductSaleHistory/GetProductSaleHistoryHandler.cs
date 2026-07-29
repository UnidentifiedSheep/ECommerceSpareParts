using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Application.Extensions.QueryExtensions;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetProductSaleHistory;

public record GetProductSaleHistoryQuery(
    int ProductId,
    Pagination Pagination,
    string? StorageName,
    Guid? OrganizationId,
    int? CurrencyId,
    string? SortBy) : IQuery<GetProductSaleHistoryResult>;

public record GetProductSaleHistoryResult(
    IReadOnlyList<ProductSaleHistoryDto> History);

public class GetProductSaleHistoryHandler(
    IReadRepository<SaleContent, int> repository,
    IProjectionProvider<SaleContent, ProductSaleHistoryDto> projectionProvider)
    : IQueryHandler<GetProductSaleHistoryQuery, GetProductSaleHistoryResult>
{
    public async Task<GetProductSaleHistoryResult> Handle(
        GetProductSaleHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query
            .CompletedSales()
            .Where(x => x.ProductId == request.ProductId);

        if (!string.IsNullOrWhiteSpace(request.StorageName))
            query = query.Where(x => x.Sale.StorageName == request.StorageName);

        if (request.OrganizationId.HasValue)
            query = query.Where(x => x.Sale.OrganizationId == request.OrganizationId);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.Sale.CurrencyId == request.CurrencyId);

        var result = await query
            .SortBy(request.SortBy)
            .Project(projectionProvider)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetProductSaleHistoryResult(result);
    }
}
