using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Pricing.Api.GraphQl.DataLoaders;
using Pricing.Api.GraphQl.Types;
using Pricing.Api.GraphQl.Types.Inputs;
using Pricing.Application.Handlers.Markup.GetMarkups;
using MediatR;

namespace Pricing.Api.GraphQl.Queries;

public sealed class MarkupGroupQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.MARKUP_GET)]
	public async Task<GqlMarkupGroup?> GetMarkupGroupByIdAsync(
		IMarkupGroupByIdDataLoader loader,
		int id,
		CancellationToken cancellationToken)
	{
		var group = await loader.LoadAsync(id, cancellationToken);
		return group is null ? null : new GqlMarkupGroup(group);
	}

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.MARKUP_GET)]
	public async Task<IReadOnlyList<GqlMarkupGroup>> SearchAsync(
		GqlSearchMarkupGroupsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetMarkupsQuery(input.Pagination, input.SearchTerm), cancellationToken);
		return result.Groups.Select(group => new GqlMarkupGroup(group)).ToArray();
	}
}
