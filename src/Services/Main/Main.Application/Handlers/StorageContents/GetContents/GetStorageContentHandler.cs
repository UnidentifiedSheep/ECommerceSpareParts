using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.StorageContents.GetContents;

public record GetStorageContentQuery(
    string? StorageName,
    int? ArticleId,
    PaginationModel Pagination,
    bool ShowZeroCount) : IQuery<GetStorageContentResult>;

public record GetStorageContentResult(IEnumerable<StorageContentDto> Content);

public class GetStorageContentHandler(IStorageContentRepository contentRepository)
    : IQueryHandler<GetStorageContentQuery, GetStorageContentResult>
{
    public async Task<GetStorageContentResult> Handle(GetStorageContentQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var size = request.Pagination.Size;
        var result = await contentRepository
            .GetStorageContents(request.StorageName, request.ArticleId, page, size,
                request.ShowZeroCount, false, cancellationToken);
        return new GetStorageContentResult(result.Adapt<List<StorageContentDto>>());
    }
}