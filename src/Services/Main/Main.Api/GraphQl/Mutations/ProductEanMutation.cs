using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Application.Handlers.ProductEans.CreateProductEan;
using Main.Application.Handlers.ProductEans.DeleteProductEan;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class ProductEanMutation
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_EANS_EDIT)]
	public async Task<bool> CreateAsync(
		ISender sender,
		GqlProductEanInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new CreateProductEanCommand(input.ProductId, input.Ean),
			cancellationToken);
		return true;
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.ARTICLE_EANS_EDIT)]
	public async Task<bool> DeleteAsync(
		ISender sender,
		GqlProductEanInput input,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new DeleteProductEanCommand(input.ProductId, input.Ean),
			cancellationToken);
		return true;
	}
}
