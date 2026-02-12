using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchase;

public record GetPurchasesQuery(DateTime RangeStartDate, DateTime RangeEndDate, PaginationModel Pagination,
    Guid? SupplierId, int? CurrencyId, string? SortBy, string? SearchTerm) : IQuery<GetPurchasesResult>;

public record GetPurchasesResult(IEnumerable<PurchaseDto> Purchases);

public class GetPurchasesHandler(IPurchaseRepository purchaseRepository)
    : IQueryHandler<GetPurchasesQuery, GetPurchasesResult>
{
    public async Task<GetPurchasesResult> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result = await purchaseRepository.GetPurchases(request.RangeStartDate, request.RangeEndDate, page, size,
            request.SupplierId,
            request.CurrencyId, request.SortBy, request.SearchTerm, false, cancellationToken);
        return new GetPurchasesResult(result.Adapt<List<PurchaseDto>>());
    }
}