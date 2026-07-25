using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public record GetFullSaleQuery(Guid SaleId) : IQuery<GetFullSaleResult>;

public record GetFullSaleResult(SaleDto Sale, IEnumerable<SaleContentDto> Contents);

public class GetFullSaleHandler(
    IReadRepository<Sale, Guid> readRepository,
    IProjectionProvider<Sale, SaleDto> saleProjection,
    IProjectionProvider<SaleContent, SaleContentDto> contentProjection
)
    : IQueryHandler<GetFullSaleQuery, GetFullSaleResult>
{
    public async Task<GetFullSaleResult> Handle(
        GetFullSaleQuery request,
        CancellationToken cancellationToken)
    {
        var saleToDto = saleProjection.Projection;
        var saleContentToDto = contentProjection.Projection;

        var result = await readRepository
                         .Query
                         .Where(x => x.Id == request.SaleId)
                         .AsExpandable()
                         .Select(x => new
                         {
                             sale = saleToDto.Invoke(x),
                             contents = x.Contents.Select(z => saleContentToDto.Invoke(z))
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new SaleNotFoundException(request.SaleId);

        return new GetFullSaleResult(result.sale, result.contents);
    }
}
