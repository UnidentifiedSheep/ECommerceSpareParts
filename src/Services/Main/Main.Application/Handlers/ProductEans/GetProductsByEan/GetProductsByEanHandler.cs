using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEans.GetProductsByEan;

public record GetProductsByEanQuery(string Ean, Pagination Pagination)
	: IQuery<GetProductsByEanResult>;

public record GetProductsByEanResult(IReadOnlyList<ProductDto> Products);

public class GetProductsByEanHandler(
	IReadRepository<Product, int> repository,
	IProjectionProvider<Product, ProductDto> projection)
	: IQueryHandler<GetProductsByEanQuery, GetProductsByEanResult>
{
	public async Task<GetProductsByEanResult> Handle(
		GetProductsByEanQuery request,
		CancellationToken cancellationToken)
	{
		var ean = request.Ean.Trim();
		var products = await repository.Query
			.Where(x => x.Eans.Any(productEan => productEan.Ean == ean))
			.OrderBy(x => x.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetProductsByEanResult(products);
	}
}
