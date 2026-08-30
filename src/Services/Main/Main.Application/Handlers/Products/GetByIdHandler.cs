using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;

namespace Main.Application.Handlers.Products;

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
