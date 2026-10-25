using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types.Inputs.User;
using Main.Api.GraphQl.Types.User;
using Main.Application.Handlers.Users.GetUsers;
using Main.Enums;
using MediatR;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.Queries;

public sealed class UserQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.USERS_GET)]
	public async Task<GqlUser?> GetByIdAsync(
		IUserByIdDataLoader loader,
		Guid id,
		CancellationToken cancellationToken)
	{
		var user = await loader.LoadAsync(id, cancellationToken);
		return user is null ? null : new GqlUser(user);
	}

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.USERS_GET)]
	public async Task<IReadOnlyList<GqlUser>> SearchAsync(
		GqlSearchUsersInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetUsersQuery(
				input.Pagination,
				input.SearchTerm,
				input.SimilarityLevel,
				input.Name,
				input.Surname,
				input.Email,
				input.Phone,
				input.UserName,
				input.Id,
				input.Description,
				input.Roles,
				input.SearchMethod ?? GeneralSearchStrategy.General),
			cancellationToken);

		return result.Users.Select(user => new GqlUser(user)).ToArray();
	}
}
