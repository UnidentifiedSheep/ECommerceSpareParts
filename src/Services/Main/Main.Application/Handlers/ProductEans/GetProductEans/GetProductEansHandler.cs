using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEans.GetProductEans;

public record GetProductEansQuery(int ProductId, Pagination Pagination)
	: IQuery<GetProductEansResult>;

public record GetProductEansResult(IReadOnlyList<string> Eans);

public class GetProductEansHandler(
	IReadRepository<ProductEan, ProductEanKey> repository)
	: IQueryHandler<GetProductEansQuery, GetProductEansResult>
{
	public async Task<GetProductEansResult> Handle(
		GetProductEansQuery request,
		CancellationToken cancellationToken)
	{
		var eans = await repository.Query
			.Where(x => x.ProductId == request.ProductId)
			.OrderBy(x => x.Ean)
			.Select(x => x.Ean)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetProductEansResult(eans);
	}
}
