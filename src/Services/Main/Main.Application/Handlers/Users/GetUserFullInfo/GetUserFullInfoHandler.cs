using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Exceptions.Auth;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Entities;
using Main.Entities.User;
using Mapster;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;

public record GetUserFullInfoResult(
    UserInfoDto? UserInfo,
    List<FullEmailDto> Emails,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public class GetUserFullInfoHandler(
    IUserRepository userRepository,
    IUserService userService)
    : IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
    public async Task<GetUserFullInfoResult> Handle(GetUserFullInfoQuery request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<User, Guid>()
            {
                Data = request.UserId
            }.WithTracking(false)
            .WithInclude(x => x.UserInfo)
            .WithInclude(x => x.UserEmails)
            .WithInclude(x => x.UserRoles);
        
        var user = await userRepository.GetUserByIdAsync(queryOptions, cancellationToken) ??
                   throw new UserNotFoundException(request.UserId);
        
        var (roles, permissions) = await userService
            .GetUserRolesAndPermissionsAsync(request.UserId, cancellationToken);
        

        return new GetUserFullInfoResult(
            user.UserInfo.Adapt<UserInfoDto?>(),
            user.UserEmails.Adapt<List<FullEmailDto>>(),
            roles,
            permissions);
    }
}