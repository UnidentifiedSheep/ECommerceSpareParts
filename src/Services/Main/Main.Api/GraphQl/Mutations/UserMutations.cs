using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using HotChocolate.Authorization;
using Main.Api.GraphQl.Types.Inputs.User;
using Main.Api.GraphQl.Types.User;
using Main.Application.Handlers.Auth.AddPermissionsToUser;
using Main.Application.Handlers.Auth.AddRolesToUser;
using Main.Application.Handlers.Auth.EmailVerification;
using Main.Application.Handlers.Auth.RemovePermissionsFromUser;
using Main.Application.Handlers.Auth.RemoveRolesFromUser;
using Main.Application.Handlers.StorageOwners.AddStorageToUser;
using Main.Application.Handlers.StorageOwners.DeleteStorageFromUser;
using Main.Application.Handlers.Users.AddEmailToUser;
using Main.Application.Handlers.Users.CreateUser;
using Main.Application.Handlers.Users.EditUserInfo;
using Main.Application.Handlers.Users.MakeEmailPrimary;
using Main.Application.Handlers.Users.RemoveEmailFromUser;
using MediatR;
using Security.Core.Interfaces;

namespace Main.Api.GraphQl.Mutations;

public sealed class UserMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.USERS_CREATE)]
	public async Task<GqlUser> CreateAsync(
		GqlCreateUserInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new CreateUserCommand(
			input.UserName,
			input.Password,
			input.UserInfo.ToDto(),
			input.Emails.Select(email => email.ToDto()).ToArray(),
			input.Phones.Select(phone => phone.ToDto()).ToArray(),
			input.Roles), cancellationToken);

		return new GqlUser(result.UserId);
	}

	[GraphQLName("editInfo")]
	[RequireAllPermissions(PermissionCodes.USERS_CREATE)]
	public async Task<GqlUser> EditInfoAsync(
		GqlEditUserInfoInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new EditUserInfoCommand(input.UserId, input.UserInfo.ToDto()), cancellationToken);

		return new GqlUser(result.UserId);
	}

	[GraphQLName("addEmail")]
	[RequireAllPermissions(PermissionCodes.USERS_MAILS_CREATE)]
	public async Task<GqlUser> AddEmailAsync(
		GqlAddUserEmailInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new AddEmailToUserCommand(input.UserId, input.Email, input.Type), cancellationToken);

		return new GqlUser(result.UserId);
	}

	[GraphQLName("removeEmail")]
	[RequireAllPermissions(PermissionCodes.USERS_MAILS_CREATE)]
	public async Task<GqlUser> RemoveEmailAsync(
		GqlUserEmailInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RemoveEmailFromUserCommand(input.UserId, input.Email), cancellationToken);
		return new GqlUser(input.UserId);
	}

	[GraphQLName("requestEmailVerification")]
	[RequireAllPermissions(PermissionCodes.USERS_MAILS_CREATE)]
	public async Task<bool> RequestEmailVerificationAsync(
		GqlUserEmailInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RequestEmailVerificationCommand(input.UserId, input.Email), cancellationToken);
		return true;
	}

	[GraphQLName("makeEmailPrimary")]
	[RequireAllPermissions(PermissionCodes.USERS_MAILS_CREATE)]
	public async Task<GqlUser> MakeEmailPrimaryAsync(
		GqlUserEmailInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new MakeEmailPrimaryCommand(input.UserId, input.Email), cancellationToken);
		return new GqlUser(input.UserId);
	}

	[GraphQLName("addRoles")]
	[RequireAllPermissions(PermissionCodes.USERS_ROLES_CREATE)]
	public async Task<GqlUser> AddRolesAsync(
		GqlUserRolesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new AddRolesToUserCommand(input.UserId, input.Roles), cancellationToken);
		return new GqlUser(result.UserId);
	}

	[GraphQLName("removeRoles")]
	[RequireAllPermissions(PermissionCodes.USERS_ROLES_CREATE)]
	public async Task<GqlUser> RemoveRolesAsync(
		GqlUserRolesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new RemoveRolesFromUserCommand(input.UserId, input.Roles), cancellationToken);
		return new GqlUser(result.UserId);
	}

	[GraphQLName("addPermissions")]
	[RequireAllPermissions(PermissionCodes.USERS_PERMISSIONS_CREATE)]
	public async Task<GqlUser> AddPermissionsAsync(
		GqlUserPermissionsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new AddPermissionsToUserCommand(input.UserId, input.Permissions), cancellationToken);
		return new GqlUser(result.UserId);
	}

	[GraphQLName("removePermissions")]
	[RequireAllPermissions(PermissionCodes.USERS_PERMISSIONS_CREATE)]
	public async Task<GqlUser> RemovePermissionsAsync(
		GqlUserPermissionsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new RemovePermissionsFromUserCommand(input.UserId, input.Permissions), cancellationToken);
		return new GqlUser(result.UserId);
	}

	[GraphQLName("addStorage")]
	[RequireAllPermissions(PermissionCodes.USERS_STORAGES_ADD)]
	public async Task<GqlUser> AddStorageAsync(
		GqlUserStorageInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new AddStorageToUserCommand(input.UserId, input.StorageCode), cancellationToken);
		return new GqlUser(input.UserId);
	}

	[GraphQLName("removeStorage")]
	[RequireAllPermissions(PermissionCodes.USERS_STORAGES_DELETE)]
	public async Task<GqlUser> RemoveStorageAsync(
		GqlUserStorageInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteStorageFromUserCommand(input.UserId, input.StorageCode), cancellationToken);
		return new GqlUser(input.UserId);
	}

	[GraphQLName("requestMyEmailVerification")]
	[Authorize]
	public async Task<bool> RequestMyEmailVerificationAsync(
		string email,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new RequestEmailVerificationCommand(userContext.UserId, email), cancellationToken);
		return true;
	}

	[GraphQLName("makeMyEmailPrimary")]
	[Authorize]
	public async Task<GqlUser> MakeMyEmailPrimaryAsync(
		string email,
		IUserContext userContext,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new MakeEmailPrimaryCommand(userContext.UserId, email), cancellationToken);
		return new GqlUser(userContext.UserId);
	}
}
