using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Organization;
using Main.Application.Handlers.Organizations;
using Main.Application.Handlers.Organizations.GetOrganizations;
using MediatR;

namespace Main.Api.GraphQl.Queries;

public sealed class OrganizationQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_GET)]
	public async Task<GqlOrganization?> GetByIdAsync(
		Guid id,
		IOrganizationByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var organization = await loader.LoadAsync(id, cancellationToken);
		return organization is null ? null : new GqlOrganization(organization);
	}

	[GraphQLName("bySystemName")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_GET)]
	public async Task<GqlOrganization> GetBySystemNameAsync(
		string systemName,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetOrganizationQuery(systemName), cancellationToken);
		return new GqlOrganization(result.Organization.Id);
	}

	[GraphQLName("list")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_GET)]
	public async Task<IReadOnlyList<GqlOrganization>> ListAsync(
		GqlGetOrganizationsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetOrganizationsQuery(
				input.Pagination,
				input.SortBy?.Select(sort => sort.ToSortExpression()).ToArray() ?? [],
				input.SearchTerm,
				input.UserId,
				input.Ids ?? [],
				input.Types ?? [],
				input.ShowHidden ?? false),
			cancellationToken);

		return result.Organizations.Select(organization => new GqlOrganization(organization)).ToArray();
	}

	[GraphQLName("isSystemNameAvailable")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_CREATE)]
	public async Task<bool> IsSystemNameAvailableAsync(
		string systemName,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new IsOrganizationSystemNameAvailableQuery(systemName), cancellationToken)).IsAvailable;
}
