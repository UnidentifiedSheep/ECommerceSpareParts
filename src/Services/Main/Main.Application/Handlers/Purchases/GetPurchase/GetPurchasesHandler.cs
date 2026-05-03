using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Handlers.Purchases.GetPurchase;

public record GetPurchasesQuery(
    DateTime RangeStartDate,
    DateTime RangeEndDate,
    Pagination Pagination,
    Guid? SupplierId,
    int? CurrencyId,
    string? SortBy,
    string? SearchTerm) : IQuery<GetPurchasesResult>;

public record GetPurchasesResult(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesHandler()
    : IQueryHandler<GetPurchasesQuery, GetPurchasesResult>
{
    public async Task<GetPurchasesResult> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}