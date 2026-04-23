using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Projections;
using Main.Entities.Storage;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageContents.GetContents;

public record GetStorageContentQuery(
    string? StorageName,
    int? ProductId,
    PaginationModel Pagination,
    bool ShowZeroCount) : IQuery<GetStorageContentResult>;

public record GetStorageContentResult(IEnumerable<StorageContentDto> Content);

public class GetStorageContentHandler(
    IReadRepository<StorageContent, int> repository)
    : IQueryHandler<GetStorageContentQuery, GetStorageContentResult>
{
    public async Task<GetStorageContentResult> Handle(
        GetStorageContentQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId);

        if (!string.IsNullOrWhiteSpace(request.StorageName))
            query = query.Where(x => x.StorageName == request.StorageName);

        if (!request.ShowZeroCount)
            query = query.Where(x => x.Count != 0);

        var result = await query
            .AsExpandable()
            .Select(StorageContentProjections.ToStorageContentDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetStorageContentResult(result);
    }
}