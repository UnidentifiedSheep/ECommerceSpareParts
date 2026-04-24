using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Projections;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Storages.GetStorageByName;

public record GetStorageByNameQuery(string StorageName) : IQuery<GetStorageByNameResult>;

public record GetStorageByNameResult(StorageDto Storage);

public class GetStorageByNameHandler(
    IReadRepository<Storage, string> repository)
    : IQueryHandler<GetStorageByNameQuery, GetStorageByNameResult>
{
    public async Task<GetStorageByNameResult> Handle(GetStorageByNameQuery request, CancellationToken cancellationToken)
    {
        var storage = await repository.Query
                          .AsExpandable()
                          .Select(StorageProjections.StorageProjection)
                          .FirstOrDefaultAsync(x => x.Name == request.StorageName.Trim(), cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);

        return new GetStorageByNameResult(storage);
    }
}