using Extensions;
using Main.Application.Dtos.Users;
using Mapster;

using CoreUserInfo = Abstractions.Models.UserInfo;
using CoreUser = Abstractions.Models.User;
using User = Main.Entities.User.User;
using UserInfo = Main.Entities.User.UserInfo;

namespace Main.Application.Configs.Mapster;

public static class MapsterUserConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<User, CoreUser>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.UserName, s => s.UserName);

        TypeAdapterConfig<UserInfo, CoreUserInfo>.NewConfig()
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname);
        
        TypeAdapterConfig<UserDto, CoreUser>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.UserName, s => s.UserName);

        TypeAdapterConfig<UserInfoDto, CoreUserInfo>.NewConfig()
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname);
        
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.UserName, s => s.UserName)
            .Map(d => d.LastLoginAt, s => s.LastLoginAt)
            .Map(d => d.AccessFailedCount, s => s.AccessFailedCount)
            .Map(d => d.UpdatedAt, s => s.UpdatedAt)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.LockoutEnd,  s => s.LockoutEnd)
            .Map(d => d.TwoFactorEnabled, s => s.TwoFactorEnabled)
            .Map(d => d.NormalizedUserName, s => s.NormalizedUserName)
            .Map(d => d.PasswordHash, s => s.PasswordHash)
            .Map(d => d.UserInfo, s => s.UserInfo);
        
        TypeAdapterConfig<UserDto, User>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.UserName, s => s.UserName)
            .Map(d => d.LastLoginAt, s => s.LastLoginAt)
            .Map(d => d.AccessFailedCount, s => s.AccessFailedCount)
            .Map(d => d.UpdatedAt, s => s.UpdatedAt)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.LockoutEnd,  s => s.LockoutEnd)
            .Map(d => d.TwoFactorEnabled, s => s.TwoFactorEnabled)
            .Map(d => d.NormalizedUserName, s => s.NormalizedUserName)
            .Map(d => d.PasswordHash, s => s.PasswordHash)
            .Map(d => d.UserInfo, s => s.UserInfo);
        
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(d => d.Name, s => s.UserInfo == null ? "UNKNOWN" : s.UserInfo.Name)
            .Map(d => d.Surname, s => s.UserInfo == null ? "UNKNOWN" : s.UserInfo.Surname)
            .Map(d => d.UserName, s => s.UserName)
            .Map(d => d.IsSupplier, s => s.UserInfo != null && s.UserInfo.IsSupplier)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Description, s => s.UserInfo == null ? null : s.UserInfo.Description);

        TypeAdapterConfig<UserInfoDto, UserInfo>.NewConfig()
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname)
            .Map(d => d.IsSupplier, s => s.IsSupplier)
            .Map(d => d.SearchColumn, s => $"{s.Name} {s.Surname} {s.Description}".ToNormalized());

        TypeAdapterConfig<UserInfo, UserInfoDto>.NewConfig()
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Name, s => s.Name)
            .Map(d => d.Surname, s => s.Surname)
            .Map(d => d.IsSupplier, s => s.IsSupplier);
    }
}