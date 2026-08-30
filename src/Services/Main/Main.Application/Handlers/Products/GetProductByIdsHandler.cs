using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;

namespace Main.Application.Handlers.Products;

public record GetProductByIdsQuery(IEnumerable<int> Ids) : IQuery<GetProductByIdsResult>;

public record GetProductByIdsResult(IReadOnlyList<ProductDto> Products);

public class GetProductByIdsHandler(IProductProvider productProvider)
	: IQueryHandler<GetProductByIdsQuery, GetProductByIdsResult>
{
	public async Task<GetProductByIdsResult> Handle(
		GetProductByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var result = await productProvider.GetProductsOrSetAsync(request.Ids, cancellationToken);
		return new GetProductByIdsResult(result.Values.ToList());
	}
}
