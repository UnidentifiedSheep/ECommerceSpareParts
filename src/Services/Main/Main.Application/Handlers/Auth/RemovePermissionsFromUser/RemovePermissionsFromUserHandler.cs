using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Entities.Auth;
using Extensions;
using Main.Entities.Exceptions;

namespace Main.Application.Handlers.Auth.RemovePermissionsFromUser;

[Transactional(retryDelayMs: 20, retryCount: 2)]
[AutoSave]
public sealed record RemovePermissionsFromUserCommand : ICommand<RemovePermissionsFromUserResult>
{
	public RemovePermissionsFromUserCommand(Guid userId, IEnumerable<string> names)
	{
		UserId = userId;
		PermissionNames = names.Select(name => name.Trim().ToNormalizedPermission()).Distinct(StringComparer.Ordinal).ToArray();
	}

	public Guid UserId { get; }

	public IReadOnlyList<string> PermissionNames { get; }
}

public sealed record RemovePermissionsFromUserResult(Guid UserId, IReadOnlyList<string> RemovedPermissions);

public sealed class RemovePermissionsFromUserHandler(
	IRepository<UserPermission, (Guid, string)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<RemovePermissionsFromUserCommand, RemovePermissionsFromUserResult>
{
	public async Task<RemovePermissionsFromUserResult> Handle(
		RemovePermissionsFromUserCommand request,
		CancellationToken cancellationToken)
	{
		if (request.PermissionNames.Count == 0)
			return new RemovePermissionsFromUserResult(request.UserId, []);

		var models = await repository.ListAsync(
			Criteria<UserPermission>.New()
				.Where(x => x.UserId == request.UserId && request.PermissionNames.Contains(x.Permission))
				.Track()
				.Build(), cancellationToken);
		var found = models.Select(x => x.Permission).ToHashSet(StringComparer.Ordinal);
		foreach (var name in request.PermissionNames)
			if (!found.Contains(name))
				throw new UserPermissionNotFound(request.UserId, name);

		unitOfWork.RemoveRange(models);

		return new RemovePermissionsFromUserResult(request.UserId, request.PermissionNames);
	}
}
