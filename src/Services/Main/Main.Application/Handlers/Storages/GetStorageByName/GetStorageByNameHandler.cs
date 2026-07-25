using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Storages.GetStorageByName;

public record GetStorageByNameQuery(string StorageName) : IQuery<GetStorageByNameResult>;

public record GetStorageByNameResult(StorageDto Storage);

public class GetStorageByNameHandler(
    IReadRepository<Storage, string> repository,
    IProjectionProvider<Storage, StorageDto> projection
)
    : IQueryHandler<GetStorageByNameQuery, GetStorageByNameResult>
{
    public async Task<GetStorageByNameResult> Handle(
        GetStorageByNameQuery request,
        CancellationToken cancellationToken)
    {
        var storage = await repository.Query
                          .Project(projection)
                          .FirstOrDefaultAsync(x => x.Name == request.StorageName.Trim(), cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);

        return new GetStorageByNameResult(storage);
    }
}
