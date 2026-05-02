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

public class GetPurchasesHandler(IPurchaseRepository purchaseRepository)
    : IQueryHandler<GetPurchasesQuery, GetPurchasesResult>
{
    public async Task<GetPurchasesResult> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<Purchase, GetPurchaseOptionsData>
            {
                Data = new GetPurchaseOptionsData
                {
                    RangeStart = request.RangeStartDate,
                    RangeEnd =  request.RangeEndDate,
                    CurrencyId = request.CurrencyId,
                    SearchTerm = request.SearchTerm,
                    SupplierId = request.SupplierId
                }
            }
            .WithPage(request.Pagination.Page)
            .WithSize(request.Pagination.Size)
            .WithInclude(x => x.Transaction)
            .WithInclude(x => x.Supplier)
            .WithInclude(x => x.Supplier.UserInfo)
            .WithInclude(x => x.Currency)
            .WithSorting(request.SortBy);
        
        var result = await purchaseRepository.GetPurchases(options, cancellationToken);
        return new GetPurchasesResult(result.Adapt<List<PurchaseDto>>());
    }
}