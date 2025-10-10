using Application.Common.Interfaces;
using Core.Models;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Storages.GetStorage;

public record GetStoragesQuery(PaginationModel Pagination, string? SearchTerm) : IQuery<GetStoragesResult>;

public record GetStoragesResult(IEnumerable<StorageDto> Storages);

public class GetStoragesHandler(IStoragesRepository repository) : IQueryHandler<GetStoragesQuery, GetStoragesResult>
{
    public async Task<GetStoragesResult> Handle(GetStoragesQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetStoragesAsync(request.SearchTerm, request.Pagination.Page,
            request.Pagination.Size, false, cancellationToken);
        return new GetStoragesResult(result.Adapt<List<StorageDto>>());
    }
}