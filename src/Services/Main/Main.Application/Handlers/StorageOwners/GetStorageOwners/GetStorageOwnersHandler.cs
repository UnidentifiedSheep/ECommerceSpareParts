using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Projections;
using Main.Entities;
using Main.Entities.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageOwners.GetStorageOwners;

public record GetStorageOwnersQuery(string Name, PaginationModel Pagination) : IQuery<GetStorageOwnersResult>;

public record GetStorageOwnersResult(IReadOnlyList<UserDto> Owners);

public class GetStorageOwnersHandler(
    IReadRepository<StorageOwner, (string, Guid)> repository) 
    : IQueryHandler<GetStorageOwnersQuery, GetStorageOwnersResult>
{
    public async Task<GetStorageOwnersResult> Handle(GetStorageOwnersQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.StorageName == request.Name)
            .OrderByDescending(x => x.UserId)
            .Select(x => x.User)
            .AsExpandable()
            .Select(UserProjections.UserProjection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetStorageOwnersResult(result);
    }
}