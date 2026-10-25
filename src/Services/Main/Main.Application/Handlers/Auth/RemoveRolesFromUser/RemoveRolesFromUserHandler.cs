using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Auth;
using Main.Entities.Exceptions;

namespace Main.Application.Handlers.Auth.RemoveRolesFromUser;

[Transactional(retryDelayMs: 20, retryCount: 2)]
[AutoSave]
public sealed record RemoveRolesFromUserCommand : ICommand<RemoveRolesFromUserResult>
{
	public RemoveRolesFromUserCommand(Guid userId, IEnumerable<string> names)
	{
		UserId = userId;
		RoleNames = names.Select(Main.Entities.Auth.RoleNames.Normalize).Distinct(StringComparer.Ordinal).ToArray();
	}

	public Guid UserId { get; }

	public IReadOnlyList<string> RoleNames { get; }
}

public sealed record RemoveRolesFromUserResult(Guid UserId, IReadOnlyList<string> RemovedRoles);

public sealed class RemoveRolesFromUserHandler(
	IRepository<UserRole, (Guid, string)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<RemoveRolesFromUserCommand, RemoveRolesFromUserResult>
{
	public async Task<RemoveRolesFromUserResult> Handle(
		RemoveRolesFromUserCommand request,
		CancellationToken cancellationToken)
	{
		if (request.RoleNames.Count == 0)
			return new RemoveRolesFromUserResult(request.UserId, []);

		var models = await repository.ListAsync(
			Criteria<UserRole>.New()
				.Where(x => x.UserId == request.UserId && request.RoleNames.Contains(x.RoleName))
				.Track()
				.Build(), cancellationToken);
		var found = models.Select(x => x.RoleName).ToHashSet(StringComparer.Ordinal);
		foreach (var name in request.RoleNames)
			if (!found.Contains(name))
				throw new UserRoleNotFoundException(request.UserId, name);

		unitOfWork.RemoveRange(models);

		return new RemoveRolesFromUserResult(request.UserId, request.RoleNames);
	}
}
