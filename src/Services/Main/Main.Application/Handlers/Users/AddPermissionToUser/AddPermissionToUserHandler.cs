using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Extensions;
using Main.Entities;
using MediatR;

namespace Main.Application.Handlers.Users.AddPermissionToUser;

[Transactional]
public record AddPermissionToUserCommand(Guid UserId, string PermissionName) : ICommand;

public class AddPermissionToUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddPermissionToUserCommand>
{
    public async Task<Unit> Handle(AddPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        var model = new UserPermission
        {
            Permission = request.PermissionName.ToNormalized(),
            UserId = request.UserId
        };
        
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}