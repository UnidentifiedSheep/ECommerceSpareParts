using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Products.Queries;

public record GetProductSizeQuery(int ProductId) : IQuery<GetProductSizesResult>;

public record GetProductSizesResult(ProductSizeDto ProductSize);

public class GetProductSizeHandler(
	IReadRepository<ProductSize, int> context,
	IProjectionProvider<ProductSize, ProductSizeDto> projection)
	: IQueryHandler<GetProductSizeQuery, GetProductSizesResult>
{
	public async Task<GetProductSizesResult> Handle(
		GetProductSizeQuery request,
		CancellationToken cancellationToken)
	{
		var size = await context
				.Query
				.Where(x => x.ProductId == request.ProductId)
				.Project(projection)
				.FirstOrDefaultAsync(cancellationToken) ??
			throw new ProductSizesNotFoundException(request.ProductId);

		return new GetProductSizesResult(size);
	}
}
