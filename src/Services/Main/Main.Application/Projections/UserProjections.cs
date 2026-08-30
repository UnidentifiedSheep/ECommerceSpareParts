using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Enums;
using LinqKit;
using Main.Application.Dtos.Auth;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Entities.User;
using Main.Enums.Auth;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class UserDtoProjectionProvider : ProjectionProviderBase<User, UserDto>
{
	public UserDtoProjectionProvider(IProjectionProvider<UserInfo, UserInfoDto> userInfoProjection)
	{
		var userInfoToDto = userInfoProjection.Projection;

		Projection = x => new UserDto
		{
			Id = x.Id,
			UserName = x.UserName.Value,
			NormalizedUserName = x.UserName.NormalizedValue,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			TwoFactorEnabled = x.TwoFactorEnabled,
			LockoutEnd = x.LockoutEnd,
			AccessFailedCount = x.AccessFailedCount,
			LastLoginAt = x.LastLoginAt,
			UserInfo = x.UserInfo == null ? null : userInfoToDto.Invoke(x.UserInfo)
		};
	}

	public override Expression<Func<User, UserDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class UserPartyDtoProjectionProvider : ProjectionProviderBase<User, UserPartyDto>
{
	public UserPartyDtoProjectionProvider(
		IProjectionProvider<User, UserDto> userProjection,
		IProjectionProvider<User, UserPartyType> partyTypeProjection)
	{
		var userToDto = userProjection.Projection;
		var userToPartyType = partyTypeProjection.Projection;
		var systemRole = Role.System.ToNormalizedRole();

		Projection = x => new UserPartyDto
		{
			PartyType = userToPartyType.Invoke(x),
			User = x.Roles.Any(role => role.RoleName == systemRole) ? null : userToDto.Invoke(x)
		};
	}

	public override Expression<Func<User, UserPartyDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class UserInfoDtoProjectionProvider : ProjectionProviderBase<UserInfo, UserInfoDto>
{
	public override Expression<Func<UserInfo, UserInfoDto>> Projection { get; } = x => new UserInfoDto
	{
		Description = x.Description,
		Name = x.Name,
		Surname = x.Surname
	};
}

[Lifetime(Lifetime.Singleton)]
public sealed class UserEmailDtoProjectionProvider : ProjectionProviderBase<UserEmail, UserEmailDto>
{
	public override Expression<Func<UserEmail, UserEmailDto>> Projection { get; } = x => new UserEmailDto
	{
		Email = x.Email.Value,
		Confirmed = x.Confirmed,
		ConfirmedAt = x.ConfirmedAt,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		EmailType = x.EmailType,
		IsPrimary = x.IsPrimary
	};
}

[Lifetime(Lifetime.Singleton)]
public sealed class UserPhoneDtoProjectionProvider : ProjectionProviderBase<UserPhone, UserPhoneDto>
{
	public override Expression<Func<UserPhone, UserPhoneDto>> Projection { get; } = x => new UserPhoneDto
	{
		IsConfirmed = x.Confirmed,
		IsPrimary = x.IsPrimary,
		Number = x.PhoneNumber,
		Type = x.PhoneType
	};
}

[Lifetime(Lifetime.Singleton)]
public sealed class UserPartyTypeProjectionProvider : ProjectionProviderBase<User, UserPartyType>
{
	private static readonly string SystemRole = Role.System.ToNormalizedRole();

	public override Expression<Func<User, UserPartyType>> Projection { get; } = x =>
		x.Roles.Any(role => role.RoleName == SystemRole) ? UserPartyType.System : UserPartyType.User;
}
