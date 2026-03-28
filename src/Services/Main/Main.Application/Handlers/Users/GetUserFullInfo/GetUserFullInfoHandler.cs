using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Permissions;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Roles;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;

public record GetUserFullInfoResult(
    UserInfoDto? UserInfo,
    List<FullEmailDto> Emails,
    List<RoleDto> Roles,
    List<PermissionDto> Permissions);

public class GetUserFullInfoHandler(
    IUserRepository userRepository,
    IUserEmailRepository userEmailRepository,
    IUserRoleRepository userRoleRepository,
    IUserPermissionRepository userPermissionRepository)
    : IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
    private static readonly PageableQueryOptions<UserEmail> UserEmailsOptions =
        new PageableQueryOptions<UserEmail>()
            .WithTracking(false);
    
    private static readonly PageableQueryOptions<UserRole> UserRolesOptions =
        new PageableQueryOptions<UserRole>()
            .WithTracking(false)
            .WithInclude(x => x.Role);
    
    public async Task<GetUserFullInfoResult> Handle(GetUserFullInfoQuery request, CancellationToken cancellationToken)
    {
        var userInfo = await userRepository.GetUserInfo(request.UserId, QueryPresets.Default, cancellationToken);
        var emails = await userEmailRepository
            .GetUserEmailsAsync(request.UserId, UserEmailsOptions, cancellationToken);
        var roles = (await userRoleRepository
                .GetUserRolesAsync(request.UserId, UserRolesOptions, cancellationToken))
            .Select(x => x.Role);
        var additionalPermissions = await userPermissionRepository
            .GetUserPermissionsAsync(request.UserId, QueryPresets.Default, cancellationToken);

        return new GetUserFullInfoResult(userInfo.Adapt<UserInfoDto?>(), emails.Adapt<List<FullEmailDto>>(),
            roles.Adapt<List<RoleDto>>(), additionalPermissions.Adapt<List<PermissionDto>>());
    }
}