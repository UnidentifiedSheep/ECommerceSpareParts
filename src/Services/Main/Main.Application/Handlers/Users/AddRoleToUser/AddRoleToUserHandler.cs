using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Exceptions.Auth;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Notifications;
using Main.Entities;
using Main.Entities.Auth;
using Main.Entities.User;
using MediatR;

namespace Main.Application.Handlers.Users.AddRoleToUser;

[Transactional]
public record AddRoleToUserCommand(Guid UserId, string RoleName) : ICommand;

public class AddRoleToUserHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IMediator mediator,
    IUnitOfWork unitOfWork) : ICommandHandler<AddRoleToUserCommand>
{
    public async Task<Unit> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<User, Guid>
        {
            Data = request.UserId
        }.WithTracking(false);
        
        var user = await userRepository.GetUserByIdAsync(queryOptions, cancellationToken) ??
                   throw new UserNotFoundException(request.UserId);
        var role = await roleRepository.GetRoleAsync(request.RoleName, false, cancellationToken) ??
                   throw new RoleNotFoundException(request.RoleName);

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleName = role.NormalizedName
        };

        await unitOfWork.AddAsync(userRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new UserUpdatedNotification(request.UserId), cancellationToken);
        return Unit.Value;
    }
}