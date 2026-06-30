using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Auth;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Auth;

namespace Main.Application.Projections;

public static class UserProjections
{
    private static readonly string SystemRole = Role.System.ToNormalizedRole();
    
    public static readonly Expression<Func<User, UserDto>> UserProjection =
        x => new UserDto
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
            UserInfo = x.UserInfo == null
                ? null
                : UserInfoProjection.Invoke(x.UserInfo)
        };

    public static readonly Expression<Func<User, UserPartyDto>> UserPartyProjection =
        x => new UserPartyDto
        {
            PartyType = TransactionPartyTypeProjection.Invoke(x),
            User = x.Roles.Any(role => role.RoleName == SystemRole)
                ? null
                : UserProjection.Invoke(x)
        };

    public static readonly Expression<Func<UserInfo, UserInfoDto>> UserInfoProjection =
        x => new UserInfoDto
        {
            Description = x.Description,
            Name = x.Name,
            Surname = x.Surname
        };

    public static readonly Expression<Func<UserEmail, UserEmailDto>> UserEmailProjection =
        x => new UserEmailDto
        {
            Email = x.Email.Value,
            Confirmed = x.Confirmed,
            ConfirmedAt = x.ConfirmedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            EmailType = x.EmailType,
            IsPrimary = x.IsPrimary
        };

    public static readonly Expression<Func<UserPhone, UserPhoneDto>> UserPhoneProjection =
        x => new UserPhoneDto
        {
            IsConfirmed = x.Confirmed,
            IsPrimary = x.IsPrimary,
            Number = x.PhoneNumber,
            Type = x.PhoneType
        };

    public static readonly Expression<Func<User, UserPartyType>> TransactionPartyTypeProjection =
        x => x.Roles.Any(role => role.RoleName == SystemRole)
            ? UserPartyType.System
            : UserPartyType.User;
}