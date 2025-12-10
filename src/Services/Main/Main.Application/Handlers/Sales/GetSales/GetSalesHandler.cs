using Application.Common.Interfaces;
using Core.Attributes;
using Core.Models;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Users;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.Sales;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Sales.GetSales;

[ExceptionType<UserNotFoundException>]
[ExceptionType<CurrencyNotFoundException>]
public record GetSalesQuery(
    DateTime RangeStartDate,
    DateTime RangeEndDate,
    PaginationModel Pagination,
    Guid? BuyerId,
    int? CurrencyId,
    string? SortBy,
    string? SearchTerm) : IQuery<GetSalesResult>;

public record GetSalesResult(IEnumerable<SaleDto> Sales);

public class GetSalesHandler(
    ISaleRepository saleRepository,
    DbDataValidatorBase dbValidator) : IQueryHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        await ValidateData(request.BuyerId, request.CurrencyId, cancellationToken);
        var pagination = request.Pagination;
        var result = await saleRepository.GetSales(request.RangeStartDate, request.RangeEndDate, pagination.Page,
            pagination.Size, false, request.SortBy, request.SearchTerm, request.BuyerId, request.CurrencyId,
            cancellationToken);
        return new GetSalesResult(result.Adapt<List<SaleDto>>());
    }

    private async Task ValidateData(Guid? buyerId, int? currencyId, CancellationToken cancellationToken = default)
    {
        var plan = new ValidationPlan();
        if (buyerId != null)
            plan.EnsureUserExists(buyerId.Value);
        if (currencyId != null)
            plan.EnsureCurrencyExists(currencyId.Value);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}