using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Users.GetUserStorages;

public record GetUserStoragesQuery(Guid UserId, PaginationModel Pagination) : IQuery<GetUserStoragesResult>;
public record GetUserStoragesResult(List<StorageDto> Storages);

public class GetUserStoragesHandler(IStorageOwnersRepository storageOwnersRepository) : IQueryHandler<GetUserStoragesQuery, GetUserStoragesResult>
{
    public async Task<GetUserStoragesResult> Handle(GetUserStoragesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var limit = request.Pagination.Size;
        var storages = await storageOwnersRepository
            .GetUserStoragesAsync(request.UserId, page, limit, false, cancellationToken);
        return new GetUserStoragesResult(storages.Adapt<List<StorageDto>>());
    }
}