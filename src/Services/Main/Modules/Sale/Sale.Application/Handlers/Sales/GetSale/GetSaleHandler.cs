using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Sale.Exceptions;
using Main.Modules.Sale.Application.Dtos.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Sale.Application.Handlers.Sales.GetSale;

public record GetSaleQuery(Guid? SaleId, Guid? TransactionId) : IQuery<GetSaleResult>;

public record GetSaleResult(SaleDto Sale);

public class GetSaleHandler(
	IReadRepository<Entities.Sale.Sale.Sale, Guid> repository,
	IProjectionProvider<Entities.Sale.Sale.Sale, SaleDto> projection) : IQueryHandler<GetSaleQuery, GetSaleResult>
{
	public async Task<GetSaleResult> Handle(GetSaleQuery request, CancellationToken cancellationToken)
	{
		var saleId = request.SaleId;
		var transactionId = request.TransactionId;

		var dto = await repository
			.Query
			.Where(x =>
				saleId.HasValue && x.Id == saleId.Value ||
				transactionId.HasValue && x.TransactionId == transactionId.Value)
			.OrderByDescending(x => saleId.HasValue && x.Id == saleId.Value)
			.Project(projection)
			.FirstOrDefaultAsync(cancellationToken);

		return dto == null
			? throw new SaleNotFoundException(request.SaleId ?? request.TransactionId!.Value)
			: new GetSaleResult(dto);
	}
}
