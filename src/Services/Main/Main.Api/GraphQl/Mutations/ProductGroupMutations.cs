using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types.Inputs.Product;
using Main.Api.GraphQl.Types.Product;
using Main.Application.Handlers.ProductGroups.UpsertProductGroup;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class ProductGroupMutations
{
	[GraphQLName("upsert")]
	[RequireAllPermissions(PermissionCodes.ARTICLES_EDIT)]
	public async Task<GqlProductGroup> UpsertAsync(
		ISender sender,
		GqlUpsertProductGroupInput input,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpsertProductGroupCommand(input.Id, input.Name),
			cancellationToken);

		return new GqlProductGroup(result.Group);
	}
}
