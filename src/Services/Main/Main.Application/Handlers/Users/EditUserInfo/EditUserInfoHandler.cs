using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.User;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Extensions;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.User;

namespace Main.Application.Handlers.Users.EditUserInfo;

[Transactional]
[AutoSave]
public record EditUserInfoCommand(Guid UserId, UserInfoDto UserInfo) : ICommand<EditUserInfoResult>;

public record EditUserInfoResult(UserInfoDto UserInfo);

public class EditUserInfoHandler(
    IIntegrationEventScope integrationEventScope,
    IRepository<User, Guid> repository
) : ICommandHandler<EditUserInfoCommand, EditUserInfoResult>
{
    public async Task<EditUserInfoResult> Handle(
        EditUserInfoCommand request,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<User>.New()
            .Where(x => x.Id == request.UserId)
            .Include(x => x.UserInfo)
            .WhereDoesNotHaveRole(Role.System)
            .Track()
            .Build();

        var user = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
                   ?? throw new UserNotFoundException(request.UserId);

        user.SetUserInfo(
            request.UserInfo.Name,
            request.UserInfo.Surname,
            request.UserInfo.Description);

        integrationEventScope.Add(
            new UserUpdatedEvent
            {
                UserId = request.UserId
            });

        return new EditUserInfoResult(UserProjections.UserInfoProjection.AsFunc()(user.UserInfo!));
    }
}