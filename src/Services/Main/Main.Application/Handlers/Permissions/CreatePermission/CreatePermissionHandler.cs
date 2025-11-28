using Application.Common.Interfaces;
using Core.Attributes;
using Core.Extensions;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Permissions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Entities;

namespace Main.Application.Handlers.Permissions.CreatePermission;

[Transactional]
public record CreatePermissionCommand(string Name, string? Description) : ICommand<CreatePermissionResult>;
public record CreatePermissionResult(string Name);

public class CreatePermissionHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator) : ICommandHandler<CreatePermissionCommand, CreatePermissionResult>
{
    public async Task<CreatePermissionResult> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.ToNormalized();
        await Validate(name, cancellationToken);

        var permission = new Permission
        {
            Name = name,
            Description = request.Description?.Trim()
        };
        await unitOfWork.AddAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePermissionResult(permission.Name);
    }

    private async Task Validate(string name, CancellationToken token = default)
    {
        var plan = new ValidationPlan().EnsureNotExists<Permission, string>(x => x.Name, name, 
            typeof(PermissionAlreadyExistsException));
        await dbValidator.Validate(plan, true, true, token);
    }
}