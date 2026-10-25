using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using TransactionErrorKind = Enums.TransactionErrorKind;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Auth.AddRolesToUser;

[Transactional(
	retryDelayMs: 20,
	retryCount: 2,
	retryErrors: TransactionErrorKind.UniqueConstraintViolation)]
[AutoSave]
public sealed record AddRolesToUserCommand : ICommand<AddRolesToUserResult>
{
	public AddRolesToUserCommand(Guid userId, IEnumerable<string> names)
	{
		UserId = userId;
		RoleNames = names.Select(Main.Entities.Auth.RoleNames.Normalize).Distinct(StringComparer.Ordinal).ToArray();
	}

	public Guid UserId { get; }

	public IReadOnlyList<string> RoleNames { get; }
}

public sealed record AddRolesToUserResult(Guid UserId, IReadOnlyList<string> AddedRoles);

public sealed class AddRolesToUserHandler(
	IReadRepository<UserRole, (Guid, string)> repository,
	IReadRepository<Role, string> roleRepository,
	IUnitOfWork unitOfWork) : ICommandHandler<AddRolesToUserCommand, AddRolesToUserResult>
{
	public async Task<AddRolesToUserResult> Handle(
		AddRolesToUserCommand request,
		CancellationToken cancellationToken)
	{
		if (request.RoleNames.Count == 0)
			return new AddRolesToUserResult(request.UserId, []);

		var roles = await roleRepository.Query
			.Where(x => request.RoleNames.Contains(x.Name))
			.Select(x => x.Name)
			.ToListAsync(cancellationToken);
		var knownRoles = roles.ToHashSet(StringComparer.Ordinal);
		foreach (var name in request.RoleNames)
			if (!knownRoles.Contains(name))
				throw new RoleNotFoundException(name);

		var existing = await repository.Query
			.Where(x => x.UserId == request.UserId && request.RoleNames.Contains(x.RoleName))
			.Select(x => x.RoleName)
			.ToListAsync(cancellationToken);
		var existingNames = existing.ToHashSet(StringComparer.Ordinal);
		var added = request.RoleNames.Where(name => !existingNames.Contains(name)).ToArray();

		await unitOfWork.AddRangeAsync(
			added.Select(name => UserRole.Create(request.UserId, name)), cancellationToken);

		return new AddRolesToUserResult(request.UserId, added);
	}
}
