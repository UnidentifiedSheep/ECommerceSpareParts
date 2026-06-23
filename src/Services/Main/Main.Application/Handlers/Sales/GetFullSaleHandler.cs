using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public record GetFullSaleQuery(Guid SaleId) : IQuery<GetFullSaleResult>;

public record GetFullSaleResult(SaleDto Sale, IEnumerable<SaleContentDto> Contents);

public class GetFullSaleHandler(
    IReadRepository<Sale, Guid> readRepository)
    : IQueryHandler<GetFullSaleQuery, GetFullSaleResult>
{
    public async Task<GetFullSaleResult> Handle(
        GetFullSaleQuery request,
        CancellationToken cancellationToken)
    {
        var result = await readRepository
            .Query
            .Where(x => x.Id == request.SaleId)
            .AsExpandable()
            .Select(x => new
            {
                sale = SaleProjections.ToSaleDto.Invoke(x),
                contents = x.Contents.Select(z => SaleProjections.ToSaleContentDto.Invoke(z))
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new SaleNotFoundException(request.SaleId);

        return new GetFullSaleResult(result.sale, result.contents);
    }
}
