using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Permissions;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Roles;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;
public record GetUserFullInfoResult(UserInfoDto? UserInfo, List<FullEmailDto> Emails, List<RoleDto> Roles, List<PermissionDto> Permissions);

public class GetUserFullInfoHandler(IUserRepository userRepository, IUserEmailRepository userEmailRepository,
    IUserRoleRepository userRoleRepository, IUserPermissionRepository userPermissionRepository) 
    : IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
    public async Task<GetUserFullInfoResult> Handle(GetUserFullInfoQuery request, CancellationToken cancellationToken)
    {
        var userInfo = await userRepository.GetUserInfo(request.UserId, false, cancellationToken);
        var emails = await userEmailRepository
            .GetUserEmailsAsync(request.UserId, null, null, false, cancellationToken);
        var roles = (await userRoleRepository
                .GetUserRolesAsync(request.UserId, false, null, null, cancellationToken, x => x.Role))
            .Select(x => x.Role);
        var additionalPermissions = await userPermissionRepository
            .GetUserPermissionsAsync(request.UserId, false, cancellationToken);
        
        return new GetUserFullInfoResult(userInfo.Adapt<UserInfoDto?>(), emails.Adapt<List<FullEmailDto>>(), 
            roles.Adapt<List<RoleDto>>(), additionalPermissions.Adapt<List<PermissionDto>>());
    }
}