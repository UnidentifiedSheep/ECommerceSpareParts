using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Main.Entities.Auth;
using MediatR;

namespace Main.Application.Handlers.Auth.AddRoleToUser;

[Transactional]
[AutoSave]
public record AddRoleToUserCommand(Guid UserId, string RoleName) : ICommand;

public class AddRoleToUserHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddRoleToUserCommand>
{
	public async Task<Unit> Handle(AddRoleToUserCommand request, CancellationToken cancellationToken)
	{
		var userRole = UserRole.Create(request.UserId, request.RoleName);
		await unitOfWork.AddAsync(userRole, cancellationToken);
		return Unit.Value;
	}
}
