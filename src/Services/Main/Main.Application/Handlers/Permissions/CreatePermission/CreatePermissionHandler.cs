using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Permissions.CreatePermission;

[Transactional]
public record CreatePermissionCommand(string Name, string? Description) : ICommand<CreatePermissionResult>;
public record CreatePermissionResult(string Name);

public class CreatePermissionHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreatePermissionCommand, CreatePermissionResult>
{
    public async Task<CreatePermissionResult> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = new Permission
        {
            Name = request.Name.ToNormalized(),
            Description = request.Description?.Trim()
        };
        await unitOfWork.AddAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreatePermissionResult(permission.Name);
    }
}