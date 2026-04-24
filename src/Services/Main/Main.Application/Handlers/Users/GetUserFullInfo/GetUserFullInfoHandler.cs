using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions.Auth;
using Main.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;

public record GetUserFullInfoResult(
    UserDto User,
    IReadOnlyList<UserEmailDto> Emails,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public class GetUserFullInfoHandler(
    IReadRepository<User, Guid> repository,
    IUserService userService)
    : IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
    public async Task<GetUserFullInfoResult> Handle(GetUserFullInfoQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.Query
                .Where(x => x.Id == request.UserId)
                .AsExpandable()
                .Select(x => new 
                { 
                    User = UserProjections.UserProjection.InvokeEFCore(x),
                    Emails = x.Emails.Select(z => UserProjections.UserEmailProjection.InvokeEFCore(z))
                })
                .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new UserNotFoundException(request.UserId);

        var (roles, permissions) = await userService
                                       .GetUserRolesAndPermissionsAsync(request.UserId, cancellationToken)
                                   ?? throw new UserNotFoundException(request.UserId);
        

        return new GetUserFullInfoResult(
            user.User,
            user.Emails.ToList(),
            roles,
            permissions);
    }
}