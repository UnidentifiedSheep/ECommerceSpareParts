using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageContents.GetContents;

public sealed record GetProductStorageContentsItem(
    int ProductId,
    Pagination Pagination,
    string? StorageCode,
    bool ShowZeroCount
);

public record GetProductStorageContentsQuery(
    IReadOnlyCollection<GetProductStorageContentsItem> Items
) : IQuery<GetProductStorageContentsResult>;

public record GetProductStorageContentsResult(
    IReadOnlyDictionary<GetProductStorageContentsItem, IReadOnlyList<StorageContentDto>> Content);

public class GetProductStorageContentsHandler(
    IStorageContentRepository repository,
    IReadRepository<StorageContent, int> readRepository,
    IProjectionProvider<StorageContent, StorageContentDto> projection
) : IQueryHandler<GetProductStorageContentsQuery, GetProductStorageContentsResult>
{
    public async Task<GetProductStorageContentsResult> Handle(
        GetProductStorageContentsQuery request,
        CancellationToken cancellationToken)
    {
        var items = request.Items.Distinct().ToArray();
        var resultIds = items.ToDictionary(
            item => item,
            _ => (IReadOnlyList<int>)[]);

        var groups = items
            .GroupBy(item => new
            {
                item.Pagination,
                item.StorageCode,
                item.ShowZeroCount
            });

        foreach (var group in groups)
        {
            var productIds = group
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();
                
            var contents = await repository.GetByProductsAsync(
                productIds,
                group.Key.Pagination,
                group.Key.StorageCode,
                group.Key.ShowZeroCount,
                cancellationToken);

            var contentsByProductId = contents
                .GroupBy(x => x.ProductId)
                .ToDictionary(
                    x => x.Key, 
                    IReadOnlyList<int> (x) => x
                        .Select(content => content.StorageContentId)
                        .ToArray());

            foreach (var item in group)
                if (contentsByProductId.TryGetValue(item.ProductId, out var productContents))
                    resultIds[item] = productContents;
        }

        var contentIds = resultIds.Values
            .SelectMany(x => x)
            .Distinct()
            .ToList();
        
        var contentsById = contentIds.Count == 0
            ? new Dictionary<int, StorageContentDto>()
            : await readRepository.Query
                .Where(x => contentIds.Contains(x.Id))
                .Project(projection)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        return new GetProductStorageContentsResult(
            resultIds.ToDictionary(
                item => item.Key, 
                IReadOnlyList<StorageContentDto> (item) => item.Value
                    .Select(id => contentsById[id])
                    .ToArray()));
    }
}
