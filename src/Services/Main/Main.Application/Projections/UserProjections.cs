using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Application.Dtos.NotificationPreference;
using Main.Application.Dtos.Users;
using Main.Entities.User;

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
public sealed class UserNotificationPreferenceProjectionProvider(
	IContextualLocalizer localizer
	) : ProjectionProviderBase<UserNotificationPreference, UserNotificationPreferenceDto>
{
	public override Expression<Func<UserNotificationPreference, UserNotificationPreferenceDto>> Projection
		{ get; } = x => new UserNotificationPreferenceDto
		{
				UserId = x.UserId,
				ChannelName = x.ChannelName,
				Enabled = x.Enabled,
				LocalizableChannelName = GetLocalized(localizer, x.ChannelName)
		};

	private static string GetLocalized(IContextualLocalizer localizer, string channelName)
		=> localizer.TryGet(new LocalizableMessage($"{channelName}Channel"), out var value)
			? value
			: channelName;
}
