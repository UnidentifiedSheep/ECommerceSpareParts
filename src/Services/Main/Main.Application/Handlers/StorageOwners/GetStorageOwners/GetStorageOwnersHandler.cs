using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Entities.User;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;
using UserEntity = Main.Entities.User.User;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public record GetStorageOwnersQuery(string StorageCode, Pagination Pagination) : IQuery<GetStorageOwnersResult>;

public record GetStorageOwnersResult(IReadOnlyList<UserDto> Owners);

public class GetStorageOwnersHandler(
    IReadRepository<StorageOwner, (string, Guid)> repository,
    IProjectionProvider<UserEntity, UserDto> projection
)
    : IQueryHandler<GetStorageOwnersQuery, GetStorageOwnersResult>
{
    public async Task<GetStorageOwnersResult> Handle(
        GetStorageOwnersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.StorageCode == request.StorageCode)
            .OrderByDescending(x => x.UserId)
            .Select(x => x.User)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetStorageOwnersResult(result);
    }
}
