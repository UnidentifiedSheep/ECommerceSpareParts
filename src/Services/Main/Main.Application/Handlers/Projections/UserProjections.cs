using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Entities.User;

namespace Main.Application.Handlers.Projections;

public static class UserProjections
{
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
    
    public static readonly Func<User, UserDto> UserProjectionFunc =
        UserProjection.Compile();
    
    
    public static readonly Expression<Func<UserInfo, UserInfoDto>> UserInfoProjection =
        x => new UserInfoDto
        {
            Description = x.Description,
            Name = x.Name,
            Surname = x.Surname,
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
            IsPrimary = x.IsPrimary,
        };
}