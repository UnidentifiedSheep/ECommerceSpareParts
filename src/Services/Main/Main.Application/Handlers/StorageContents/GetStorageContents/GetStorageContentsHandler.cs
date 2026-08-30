using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageContents.GetStorageContents;

public sealed record GetStorageContentsQuery(
    int? ProductId,
    string? StorageCode,
    string[] SortBy,
    Pagination Pagination,
    bool ShowZeroCount
) : IQuery<GetStorageContentsResult>;

public sealed record GetStorageContentsResult(
    IReadOnlyList<StorageContentDto> Content);

public sealed class GetStorageContentsHandler(
    IReadRepository<StorageContent, int> repository,
    IProjectionProvider<StorageContent, StorageContentDto> projection
) : IQueryHandler<GetStorageContentsQuery, GetStorageContentsResult>
{
    public async Task<GetStorageContentsResult> Handle(
        GetStorageContentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(request.StorageCode))
            query = query.Where(x => x.StorageCode == request.StorageCode);

        if (!request.ShowZeroCount)
            query = query.Where(x => x.Count > 0);

        var content = await query
            .SortBy(request.SortBy)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetStorageContentsResult(content);
    }
}
