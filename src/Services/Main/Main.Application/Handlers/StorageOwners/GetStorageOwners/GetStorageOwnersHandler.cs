using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Entities.Storage;
using Mapster;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public record GetStorageOwnersQuery(string Name, PaginationModel Pagination) : IQuery<GetStorageOwnersResult>;

public record GetStorageOwnersResult(IReadOnlyList<UserDto> Owners);

public class GetStorageOwnersHandler(
    IStorageOwnersRepository storageOwnersRepository) 
    : IQueryHandler<GetStorageOwnersQuery, GetStorageOwnersResult>
{
    public async Task<GetStorageOwnersResult> Handle(GetStorageOwnersQuery request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<StorageOwner, string>()
            {
                Data = request.Name,
            }
            .WithTracking(false)
            .WithOrderBy(x => x.OwnerId)
            .WithPage(request.Pagination.Page)
            .WithSize(request.Pagination.Size)
            .WithInclude(x => x.Owner)
            .WithInclude(x => x.Owner.UserInfo);

        var result = await storageOwnersRepository.GetStorageOwnersAsync(queryOptions, cancellationToken);
        var owners = result.Select(x => x.Owner.Adapt<UserDto>()).ToList();
        return new GetStorageOwnersResult(owners);
    }
}