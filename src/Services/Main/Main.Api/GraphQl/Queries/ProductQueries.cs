using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Dtos.Product;

namespace Main.Api.GraphQl.Queries;

public sealed class ProductQueries
{
	[GraphQLName("byId")]
	[Lookup]
	public async Task<GqlProduct?> GetProductByIdAsync(
		IProductByIdDataLoader loader,
		int id,
		CancellationToken cancellationToken)
	{
		var product = await loader.LoadAsync(id, cancellationToken);
		return product is null ? null : new GqlProduct(product);
	}

	[GraphQLName("byIds")]
	public async Task<IReadOnlyList<GqlProduct>> GetProductsByIdsAsync(
		IProductByIdDataLoader loader,
		IReadOnlyCollection<int> ids,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(ids, cancellationToken))
			.OfType<ProductDto>()
			.Select(x => new GqlProduct(x))
			.ToList();
}
