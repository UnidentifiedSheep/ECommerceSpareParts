using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using TransactionErrorKind = Enums.TransactionErrorKind;
using Main.Entities.Auth;
using Extensions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth.AddPermissionsToUser;

[Transactional(
	retryDelayMs: 20,
	retryCount: 2,
	retryErrors: TransactionErrorKind.UniqueConstraintViolation)]
[AutoSave]
public sealed record AddPermissionsToUserCommand : ICommand<AddPermissionsToUserResult>
{
	public AddPermissionsToUserCommand(Guid userId, IEnumerable<string> names)
	{
		UserId = userId;
		PermissionNames = names
			.Select(x => x.Trim().ToNormalizedPermission())
			.Distinct(StringComparer.Ordinal)
			.ToArray();
	}

	public Guid UserId { get; }

	public IReadOnlyList<string> PermissionNames { get; }
}

public sealed record AddPermissionsToUserResult(Guid UserId, IReadOnlyList<string> AddedPermissions);

public sealed class AddPermissionsToUserHandler(
	IReadRepository<UserPermission, (Guid, string)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<AddPermissionsToUserCommand, AddPermissionsToUserResult>
{
	public async Task<AddPermissionsToUserResult> Handle(
		AddPermissionsToUserCommand request,
		CancellationToken cancellationToken)
	{
		if (request.PermissionNames.Count == 0)
			return new AddPermissionsToUserResult(request.UserId, []);

		var existing = await repository.Query
			.Where(x => x.UserId == request.UserId && request.PermissionNames.Contains(x.Permission))
			.Select(x => x.Permission)
			.ToListAsync(cancellationToken);
		var existingNames = existing.ToHashSet(StringComparer.Ordinal);
		var added = request.PermissionNames.Where(name => !existingNames.Contains(name)).ToArray();

		await unitOfWork.AddRangeAsync(
			added.Select(name => UserPermission.Create(request.UserId, name)), cancellationToken);

		return new AddPermissionsToUserResult(request.UserId, added);
	}
}
