using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.StorageContents.GetProductStorageContents;

public sealed record GetProductStorageContentsItem(
	int ProductId,
	Pagination Pagination,
	string? StorageCode,
	bool ShowZeroCount);

public record GetProductStorageContentsQuery(IReadOnlyCollection<GetProductStorageContentsItem> Items)
	: IQuery<GetProductStorageContentsResult>;

public record GetProductStorageContentsResult(
	IReadOnlyDictionary<GetProductStorageContentsItem, IReadOnlyList<StorageContentDto>> Content);

public class GetProductStorageContentsHandler(
	IReadRepository<StorageContent, int> readRepository,
	IProjectionProvider<StorageContent, StorageContentDto> projection)
	: IQueryHandler<GetProductStorageContentsQuery, GetProductStorageContentsResult>
{
	public async Task<GetProductStorageContentsResult> Handle(
		GetProductStorageContentsQuery request,
		CancellationToken cancellationToken)
	{
		var items = request.Items.Distinct().ToArray();
		var result = items.ToDictionary(item => item, _ => (IReadOnlyList<StorageContentDto>)[]);

		foreach (var group in items.GroupBy(item => new { item.Pagination, item.StorageCode, item.ShowZeroCount }))
		{
			var productIds = group.Select(item => item.ProductId).ToList();
			var query = readRepository.Query
				.Where(content => productIds.Contains(content.ProductId));

			if (!string.IsNullOrWhiteSpace(group.Key.StorageCode))
				query = query.Where(content => content.StorageCode == group.Key.StorageCode);

			if (!group.Key.ShowZeroCount)
				query = query.Where(content => content.Count > 0);

			var offset = group.Key.Pagination.Page * group.Key.Pagination.Size;
			var size = group.Key.Pagination.Size;
			var contents = await query.AsExpandable()
				.GroupBy(content => content.ProductId)
				.Select(product => new
				{
					ProductId = product.Key,
					Contents = product.OrderBy(content => content.Id).Skip(offset).Take(size)
						.Select(content => projection.Projection.Invoke(content)).ToList()
				})
				.ToDictionaryAsync(product => product.ProductId, cancellationToken);

			foreach (var item in group)
				if (contents.TryGetValue(item.ProductId, out var product))
					result[item] = product.Contents;
		}

		return new GetProductStorageContentsResult(result);
	}
}
