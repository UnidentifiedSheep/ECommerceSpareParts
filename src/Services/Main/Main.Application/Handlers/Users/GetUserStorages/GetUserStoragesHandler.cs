using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users.GetUserStorages;

public record GetUserStoragesQuery(Guid UserId, Pagination Pagination) : IQuery<GetUserStoragesResult>;

public record GetUserStoragesResult(List<StorageDto> Storages);

public class GetUserStoragesHandler(
    IReadRepository<StorageOwner, (string, Guid)> storageOwnersRepository,
    IProjectionProvider<Storage, StorageDto> projection)
    : IQueryHandler<GetUserStoragesQuery, GetUserStoragesResult>
{
    public async Task<GetUserStoragesResult> Handle(
        GetUserStoragesQuery request,
        CancellationToken cancellationToken)
    {
        var storages = await storageOwnersRepository.Query
            .Where(x => x.UserId == request.UserId)
            .Select(x => x.Storage)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetUserStoragesResult(storages);
    }
}
