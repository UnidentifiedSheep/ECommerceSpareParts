using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Queries.Products.GetProductsByEan;

public record GetProductsByEanQuery(string Ean, Pagination Pagination)
	: IQuery<GetProductsByEanResult>;

public record GetProductsByEanResult(IReadOnlyList<ProductDto> Products);

public class GetProductsByEanHandler(
	IReadRepository<Entities.Product.Product.Product, int> repository,
	IProjectionProvider<Entities.Product.Product.Product, ProductDto> projection)
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
