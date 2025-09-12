using Application.Extensions;
using Application.Interfaces;
using Core.Dtos.Amw.Sales;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Mapster;

namespace Application.Handlers.Sales.GetSales;

public record GetSalesQuery(DateTime RangeStartDate, DateTime RangeEndDate, PaginationModel Pagination, string? BuyerId, 
    int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesHandler(ISaleRepository saleRepository, IUsersRepository usersRepository, 
    ICurrencyRepository currencyRepository) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        await ValidateData(request.BuyerId, request.CurrencyId, cancellationToken);
        var pagination = request.Pagination;
        var result = await saleRepository.GetSales(request.RangeStartDate, request.RangeEndDate, pagination.Page, 
            pagination.Size, false, request.SortBy, request.SearchTerm, request.BuyerId, request.CurrencyId, cancellationToken);
        return new GetSalesResult(result.Adapt<List<SaleDto>>());
    }

    private async Task ValidateData(string? buyerId, int? currencyId, CancellationToken cancellationToken = default)
    {
        if (buyerId != null)
            await usersRepository.EnsureUsersExists([buyerId], cancellationToken);
        if (currencyId != null)
            await currencyRepository.EnsureCurrenciesExists([currencyId.Value], cancellationToken);
    }
}