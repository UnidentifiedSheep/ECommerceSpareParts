using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.Products.CreateProducts;
using Main.Application.Handlers.Products.PatchProduct;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class ProductMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_CREATE)]
	public async Task<IReadOnlyList<GqlProduct>> CreateAsync(
		ISender sender,
		IReadOnlyList<GqlCreateProductInput> products,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateProductsCommand(products.Select(product => product.ToDto()).ToList()),
			cancellationToken);

		return result.CreatedIds.Select(id => new GqlProduct(id)).ToArray();
	}

	[GraphQLName("patch")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_EDIT)]
	public async Task<GqlProduct> PatchAsync(
		ISender sender,
		int productId,
		GqlPatchProductInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new PatchProductCommand(productId, input.ToDto()),
			cancellationToken);

		return new GqlProduct(productId);
	}
}
