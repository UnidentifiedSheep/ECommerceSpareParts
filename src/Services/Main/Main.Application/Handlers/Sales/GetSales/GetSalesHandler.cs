using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit.Core;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Projections;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(
    DateTime RangeStartDate,
    DateTime RangeEndDate,
    Pagination Pagination,
    Guid? BuyerId,
    int? CurrencyId,
    string? Sku,
    string? ProductName,
    string? Comment,
    string? SortBy) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesHandler(
    IReadRepository<Sale, Guid> repository
) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var fixedStart = request.RangeStartDate.Date;
        var fixedEnd = request.RangeEndDate.Date.AddDays(1);

        var query = repository.Query
            .Where(x => x.SaleDatetime >= fixedStart && x.SaleDatetime <= fixedEnd);

        if (request.CurrencyId.HasValue)
            query = query.Where(x => x.CurrencyId == request.CurrencyId);

        if (request.BuyerId.HasValue)
            query = query.Where(x => x.BuyerId == request.BuyerId);

        if (!string.IsNullOrWhiteSpace(request.Sku))
            query = query.Where(x => x.Contents.Any(z => EF.Functions
                .ILike(z.Product.Sku.NormalizedValue, $"%{request.Sku.Trim()}")));

        if (!string.IsNullOrWhiteSpace(request.ProductName))
            query = query.Where(x => x.Contents.Any(z => EF.Functions
                .ILike(z.Product.Name.Value, $"%{request.ProductName.Trim()}%")));

        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            var pattern = $"%{request.Comment}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Comment!, pattern) ||
                x.Contents.Any(z => EF.Functions.ILike(z.Comment!, pattern)));
        }

        var result = await query
            .AsExpandable()
            .Select(SaleProjections.ToSaleDto)
            .SortBy(request.SortBy)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetSalesResult(result);
    }
}