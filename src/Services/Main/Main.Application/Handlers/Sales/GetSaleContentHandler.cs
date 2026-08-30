using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public record GetSaleContentQuery(Guid Id) : IQuery<GetSaleContentResult>;

public record GetSaleContentResult(IReadOnlyList<SaleContentDto> Content);

public class GetSaleContentHandler(
	IReadRepository<SaleContent, int> repository,
	IProjectionProvider<SaleContent, SaleContentDto> projection)
	: IQueryHandler<GetSaleContentQuery, GetSaleContentResult>
{
	public async Task<GetSaleContentResult> Handle(
		GetSaleContentQuery request,
		CancellationToken cancellationToken)
	{
		var result = await repository
			.Query
			.Where(x => x.SaleId == request.Id)
			.Project(projection)
			.ToListAsync(cancellationToken);

		return result.Count == 0
			? throw new SaleNotFoundException(request.Id)
			: new GetSaleContentResult(result);
	}
}
