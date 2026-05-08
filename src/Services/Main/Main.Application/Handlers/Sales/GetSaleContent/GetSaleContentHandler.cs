using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Projections;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales.GetSaleContent;

public record GetSaleContentQuery(Guid Id) : IQuery<GetSaleContentResult>;

public record GetSaleContentResult(IReadOnlyList<SaleContentDto> Content);

public class GetSaleContentHandler(
    IReadRepository<SaleContent, int> repository)
    : IQueryHandler<GetSaleContentQuery, GetSaleContentResult>
{
    public async Task<GetSaleContentResult> Handle(GetSaleContentQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.SaleId == request.Id)
            .AsExpandable()
            .Select(SaleProjections.ToSaleContentDto)
            .ToListAsync(cancellationToken);

        return new GetSaleContentResult(result);
    }
}