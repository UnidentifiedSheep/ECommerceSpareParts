using Application.Common.Interfaces;
using Core.Attributes;
using Exceptions.Exceptions.Storages;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Storages.GetStorageByName;

[ExceptionType<StorageNotFoundException>]
public record GetStorageByNameQuery(string StorageName) : IQuery<GetStorageByNameResult>;
public record GetStorageByNameResult(StorageDto Storage);

public class GetStorageByNameHandler(IStoragesRepository repository) : IQueryHandler<GetStorageByNameQuery, GetStorageByNameResult>
{
    public async Task<GetStorageByNameResult> Handle(GetStorageByNameQuery request, CancellationToken cancellationToken)
    {
        var storage = await repository.GetStorageAsync(request.StorageName.Trim(), false, cancellationToken)
                      ?? throw new StorageNotFoundException(request.StorageName);

        return new GetStorageByNameResult(storage.Adapt<StorageDto>());
    }
}