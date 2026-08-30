using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using MediatR;

namespace Main.Application.Handlers.Auth;

[Transactional]
[AutoSave]
public record RemovePermissionFromUserCommand(Guid UserId, string PermissionName) : ICommand;

public class RemovePermissionFromUserHandler(
	IRepository<UserPermission, (Guid, string)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<RemovePermissionFromUserCommand>
{
	public async Task<Unit> Handle(
		RemovePermissionFromUserCommand request,
		CancellationToken cancellationToken)
	{
		var userPermission =
			await repository.GetById((request.UserId, request.PermissionName), cancellationToken) ??
			throw new UserPermissionNotFound(request.UserId, request.PermissionName);
		unitOfWork.Remove(userPermission);

		return Unit.Value;
	}
}
