using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;

namespace Main.Application.Handlers.Projections;

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

    public static readonly Expression<Func<User, TransactionPartyType>> TransactionPartyTypeProjection =
        x => x.Roles.Any(role => role.RoleName == SystemRole)
            ? TransactionPartyType.System
            : TransactionPartyType.User;
}