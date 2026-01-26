using Application.Common.Interfaces;
using Core.Models;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Storages.GetStorage;

public record GetStoragesQuery(PaginationModel Pagination, string? SearchTerm, StorageType? Type) : IQuery<GetStoragesResult>;

public record GetStoragesResult(IEnumerable<StorageDto> Storages);

public class GetStoragesHandler(IStoragesRepository repository) : IQueryHandler<GetStoragesQuery, GetStoragesResult>
{
    public async Task<GetStoragesResult> Handle(GetStoragesQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.GetStoragesAsync(request.SearchTerm, request.Pagination.Page,
            request.Pagination.Size, false, request.Type, cancellationToken);
        return new GetStoragesResult(result.Adapt<List<StorageDto>>());
    }
}