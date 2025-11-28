using Application.Common.Interfaces;
using Core.Attributes;
using Core.Extensions;
using Core.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;
using MediatR;

namespace Main.Application.Handlers.Users.AddPermissionToUser;

[Transactional]
public record AddPermissionToUserCommand(Guid UserId, string PermissionName) : ICommand;

public class AddPermissionToUserHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator) : ICommandHandler<AddPermissionToUserCommand>
{
    public async Task<Unit> Handle(AddPermissionToUserCommand request, CancellationToken cancellationToken)
    {
        var name = request.PermissionName.ToNormalized();
        await Validate(request.UserId, name, cancellationToken);

        var model = new UserPermission
        {
            Permission = name,
            UserId = request.UserId
        };
        
        await unitOfWork.AddAsync(model, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
    
    private async Task Validate(Guid userId, string permissionName, CancellationToken cancellationToken)
    {
        var plan = new ValidationPlan()
            .EnsureUserExists(userId)
            .EnsurePermissionExists(permissionName);
        await dbValidator.Validate(plan, true, true, cancellationToken);
    }
}