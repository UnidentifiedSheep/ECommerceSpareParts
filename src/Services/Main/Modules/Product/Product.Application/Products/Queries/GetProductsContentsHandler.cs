using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Products.Queries;

public record GetProductsContentsQuery : IQuery<GetProductsContentsResult>
{

	public GetProductsContentsQuery(IEnumerable<int> ids) => Ids = ids.Distinct().ToList();

	public GetProductsContentsQuery(int id) : this([id])
	{
	}

	public IReadOnlyList<int> Ids { get; }
}

public record GetProductsContentsResult(Dictionary<int, List<ProductContentDto>> Contents);

public class GetProductsContentsHandler(
	IReadRepository<ProductContent, (int, int)> repository,
	IProjectionProvider<Entities.Product.Product.Product, ProductDto> productProjection)
	: IQueryHandler<GetProductsContentsQuery, GetProductsContentsResult>
{
	public async Task<GetProductsContentsResult> Handle(
		GetProductsContentsQuery request,
		CancellationToken cancellationToken)
	{
		var productToDto = productProjection.Projection;

		var result = (await repository
				.Query
				.Where(x => request.Ids.Contains(x.ParentProductId))
				.AsExpandable()
				.Select(x => new
				{
					ParentId = x.ParentProductId,
					Dto = new ProductContentDto
					{
						Quantity = x.Quantity, Product = productToDto.Invoke(x.ChildProduct)
					}
				})
				.ToListAsync(cancellationToken))
			.GroupBy(x => x.ParentId, x => x.Dto)
			.ToDictionary(x => x.Key, x => x.ToList());

		return new GetProductsContentsResult(result);
	}
}
