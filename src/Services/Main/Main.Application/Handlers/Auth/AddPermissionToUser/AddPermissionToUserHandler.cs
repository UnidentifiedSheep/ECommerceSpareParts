using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.AddPermissionToUser;

[Transactional]
[AutoSave]
public record AddPermissionToUserCommand(Guid UserId, string PermissionName) : ICommand;

public class AddPermissionToUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddPermissionToUserCommand>
{
	public async Task<Unit> Handle(AddPermissionToUserCommand request, CancellationToken cancellationToken)
	{
		var model = UserPermission.Create(request.UserId, request.PermissionName);

		await unitOfWork.AddAsync(model, cancellationToken);
		return Unit.Value;
	}
}
