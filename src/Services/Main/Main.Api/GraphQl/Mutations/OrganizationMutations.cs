using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Types;
using Main.Api.GraphQl.Types.Inputs.Organization;
using Main.Application.Handlers.Balance.UpdateOrganizationFinancialProfile;
using Main.Application.Handlers.Organizations.AddOrganizationMember;
using Main.Application.Handlers.Organizations.ChangeOrganizationMemberRole;
using Main.Application.Handlers.Organizations.CreateOrganization;
using Main.Application.Handlers.Organizations.RemoveOrganizationMember;
using Main.Application.Handlers.Organizations.UpdateOrganization;
using MediatR;

namespace Main.Api.GraphQl.Mutations;

public sealed class OrganizationMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_CREATE)]
	public async Task<GqlOrganization> CreateAsync(
		GqlCreateOrganizationInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateOrganizationCommand(input.OwnerId, input.Name, input.SystemName), cancellationToken);

		return new GqlOrganization(result.OrganizationId);
	}

	[GraphQLName("update")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_EDIT)]
	public async Task<GqlOrganization> UpdateAsync(
		GqlUpdateOrganizationInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpdateOrganizationCommand(input.OrganizationId, input.ToDto()), cancellationToken);

		return new GqlOrganization(result.OrganizationId);
	}

	[GraphQLName("addMember")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_EDIT)]
	public async Task<GqlOrganization> AddMemberAsync(
		GqlOrganizationMemberRoleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddOrganizationMemberCommand(input.OrganizationId, input.UserId, input.Role), cancellationToken);

		return new GqlOrganization(input.OrganizationId);
	}

	[GraphQLName("changeMemberRole")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_EDIT)]
	public async Task<GqlOrganization> ChangeMemberRoleAsync(
		GqlOrganizationMemberRoleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new ChangeOrganizationMemberRoleCommand(input.OrganizationId, input.UserId, input.Role), cancellationToken);

		return new GqlOrganization(input.OrganizationId);
	}

	[GraphQLName("removeMember")]
	[RequireAllPermissions(PermissionCodes.ORGANIZATIONS_EDIT)]
	public async Task<GqlOrganization> RemoveMemberAsync(
		GqlOrganizationMemberInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RemoveOrganizationMemberCommand(input.OrganizationId, input.UserId), cancellationToken);

		return new GqlOrganization(input.OrganizationId);
	}

	[GraphQLName("updateFinancialProfile")]
	[RequireAllPermissions(PermissionCodes.BALANCES_FINANCES_UPDATE)]
	public async Task<GqlOrganization> UpdateFinancialProfileAsync(
		GqlUpdateOrganizationFinancialProfileInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpdateOrganizationFinancialProfileCommand(input.OrganizationId, input.ToDto()), cancellationToken);

		return new GqlOrganization(result.OrganizationId);
	}
}
