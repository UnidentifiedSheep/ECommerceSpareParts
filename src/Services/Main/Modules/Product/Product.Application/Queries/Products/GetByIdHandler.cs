using Application.Common.Interfaces.Cqrs;
using Main.Modules.Product.Application.Dtos.Product;
using Main.Modules.Product.Application.Products.Interfaces;

namespace Main.Modules.Product.Application.Queries.Products;

public record GetProductByIdQuery(int ProductId) : IQuery<GetByIdResult>;

public record GetByIdResult(ProductDto Product);

public class GetByIdHandler(IProductProvider productProvider)
	: IQueryHandler<GetProductByIdQuery, GetByIdResult>
{
	public async Task<GetByIdResult> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
	{
		var product = await productProvider.GetProductOrSetAsync(request.ProductId, cancellationToken);
		return new GetByIdResult(product);
	}
}
