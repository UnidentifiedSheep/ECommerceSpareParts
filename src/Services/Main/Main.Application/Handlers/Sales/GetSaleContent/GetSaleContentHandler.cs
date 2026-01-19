using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Sales.GetSaleContent;

public record GetSaleContentQuery(string Id) : IQuery<GetSaleContentResult>;

public record GetSaleContentResult(List<SaleContentDto> Content);

public class GetSaleContentHandler(ISaleRepository saleRepository)
    : IQueryHandler<GetSaleContentQuery, GetSaleContentResult>
{
    public async Task<GetSaleContentResult> Handle(GetSaleContentQuery request, CancellationToken cancellationToken)
    {
        var content = await saleRepository.GetSaleContent(request.Id, false, cancellationToken);
        return new GetSaleContentResult(content.Adapt<List<SaleContentDto>>());
    }
}