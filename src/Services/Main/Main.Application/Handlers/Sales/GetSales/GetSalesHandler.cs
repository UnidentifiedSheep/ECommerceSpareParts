using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Extensions;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Dtos.RepositoryOptionsData;
using Main.Application.Dtos.Sale;
using Main.Entities;
using Main.Entities.Sale;
using Mapster;

namespace Main.Application.Handlers.Sales.GetSales;

public record GetSalesQuery(
    DateTime RangeStartDate,
    DateTime RangeEndDate,
    PaginationModel Pagination,
    Guid? BuyerId,
    int? CurrencyId,
    string? SortBy,
    string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesHandler(ISaleRepository saleRepository) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<Sale, GetSalesOptionsData>()
        {
            Data = new GetSalesOptionsData
            {
                BuyerId = request.BuyerId,
                CurrencyId = request.CurrencyId,
                RangeEnd = request.RangeEndDate,
                RangeStart = request.RangeStartDate,
                SearchTerm = request.SearchTerm
            }
        }
        .WithInclude(x => x.Transaction)
        .WithInclude(x => x.Buyer)
        .WithInclude(x => x.Buyer.UserInfo)
        .WithInclude(x => x.Currency)
        .WithPage(request.Pagination.Page)
        .WithSize(request.Pagination.Size)
        .WithSorting(request.SortBy)
        .WithTracking(false);
        
        var result = await saleRepository.GetSales(options, cancellationToken);
        return new GetSalesResult(result.Adapt<List<SaleDto>>());
    }
}