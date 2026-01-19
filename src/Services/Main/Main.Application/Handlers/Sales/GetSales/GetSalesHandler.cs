using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(DateTime RangeStartDate, DateTime RangeEndDate, PaginationModel Pagination,
    Guid? BuyerId, int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesHandler(ISaleRepository saleRepository) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var result = await saleRepository.GetSales(request.RangeStartDate, request.RangeEndDate, pagination.Page,
            pagination.Size, false, request.SortBy, request.SearchTerm, request.BuyerId, request.CurrencyId,
            cancellationToken);
        return new GetSalesResult(result.Adapt<List<SaleDto>>());
    }
}