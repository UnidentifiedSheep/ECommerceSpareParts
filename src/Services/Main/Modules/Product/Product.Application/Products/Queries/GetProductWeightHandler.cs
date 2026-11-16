using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Exceptions;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;
using ProductWeightEntity = Main.Entities.Product.Product.ProductWeight;

namespace Main.Modules.Product.Application.Products.Queries;

public record GetProductWeightQuery(int ProductId) : IQuery<GetProductWeightResult>;

public record GetProductWeightResult(ProductWeightDto ProductWeight);

public class GetProductWeightHandler(
	IReadRepository<ProductWeightEntity, int> context,
	IProjectionProvider<ProductWeightEntity, ProductWeightDto> projection)
	: IQueryHandler<GetProductWeightQuery, GetProductWeightResult>
{
	public async Task<GetProductWeightResult> Handle(
		GetProductWeightQuery request,
		CancellationToken cancellationToken)
	{
		var productWeight =
			await context
				.Query
				.Where(x => x.ProductId == request.ProductId)
				.Project(projection)
				.FirstOrDefaultAsync(cancellationToken) ??
			throw new ProductWeightNotFoundException(request.ProductId);

		return new GetProductWeightResult(productWeight);
	}
}
