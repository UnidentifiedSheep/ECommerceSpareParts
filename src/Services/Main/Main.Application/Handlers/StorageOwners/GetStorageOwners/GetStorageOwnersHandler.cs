using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Application.Dtos.Users;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using UserEntity = Main.Entities.User.User;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public record GetStorageOwnersQuery(string StorageCode, Pagination Pagination)
	: IQuery<GetStorageOwnersResult>;

public record GetStorageOwnersResult(IReadOnlyList<UserDto> Owners);

public class GetStorageOwnersHandler(
	IReadRepository<StorageOwner, (string, Guid)> repository,
	IReadRepository<UserEntity, Guid> users,
	IProjectionProvider<UserEntity, UserDto> projection)
	: IQueryHandler<GetStorageOwnersQuery, GetStorageOwnersResult>
{
	public async Task<GetStorageOwnersResult> Handle(
		GetStorageOwnersQuery request,
		CancellationToken cancellationToken)
	{
		var owners = repository.Query.Where(x => x.StorageCode == request.StorageCode);

		var result = await users
			.Query
			.Where(user => owners.Any(owner => owner.UserId == user.Id))
			.OrderByDescending(user => user.Id)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetStorageOwnersResult(result);
	}
}
