using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Storages.GetStorageByCode;

public record GetStorageByCodeQuery(string StorageCode) : IQuery<GetStorageByCodeResult>;

public record GetStorageByCodeResult(StorageDto Storage);

public class GetStorageByCodeHandler(
    IReadRepository<Storage, string> repository,
    IProjectionProvider<Storage, StorageDto> projection
)
    : IQueryHandler<GetStorageByCodeQuery, GetStorageByCodeResult>
{
    public async Task<GetStorageByCodeResult> Handle(
        GetStorageByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var storage = await repository.Query
                          .Project(projection)
                          .FirstOrDefaultAsync(x => x.Code == request.StorageCode.Trim(), cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageCode);

        return new GetStorageByCodeResult(storage);
    }
}
