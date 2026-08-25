using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageContents.GetContents;

public record GetStorageContentQuery(
    string? StorageCode,
    int? ProductId,
    Pagination Pagination,
    bool ShowZeroCount
) : IQuery<GetStorageContentResult>;

public record GetStorageContentResult(IEnumerable<StorageContentDto> Content);

public class GetStorageContentHandler(
    IReadRepository<StorageContent, int> repository,
    IProjectionProvider<StorageContent, StorageContentDto> projection
)
    : IQueryHandler<GetStorageContentQuery, GetStorageContentResult>
{
    public async Task<GetStorageContentResult> Handle(
        GetStorageContentQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.ProductId.HasValue) query = query.Where(x => x.ProductId == request.ProductId);

        if (!string.IsNullOrWhiteSpace(request.StorageCode))
            query = query.Where(x => x.StorageCode == request.StorageCode);

        if (!request.ShowZeroCount) query = query.Where(x => x.Count > 0);

        var result = await query
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetStorageContentResult(result);
    }
}
