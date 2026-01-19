using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Roles;
using Exceptions.Exceptions.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.Users.AddRoleToUser;

[Transactional]
public record AddRoleToUserCommand(Guid UserId, string RoleName) : ICommand;

public class AddRoleToUserHandler(IUserRepository userRepository, IRoleRepository roleRepository, 
    IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork) : ICommandHandler<AddRoleToUserCommand>
{
    public async Task<Unit> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByIdAsync(request.UserId, cancellationToken: cancellationToken) ?? 
                   throw new UserNotFoundException(request.UserId);
        var role = await roleRepository.GetRoleAsync(request.RoleName, true, cancellationToken) ??
                   throw new RoleNotFoundException(request.RoleName);

        if (await userRoleRepository.ExistsAsync(user.Id, role.Id, cancellationToken))
            throw new UserAlreadyContainsRoleException(user.Id, role.Name);

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };

        await unitOfWork.AddAsync(userRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}