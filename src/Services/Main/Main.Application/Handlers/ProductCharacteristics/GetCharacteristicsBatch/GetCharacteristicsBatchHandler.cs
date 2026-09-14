using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductCharacteristics.GetCharacteristicsBatch;

public record GetCharacteristicsBatchQuery : IQuery<GetCharacteristicsBatchResult>
{
	public IReadOnlyList<int> ProductIds { get; }

	public GetCharacteristicsBatchQuery(IEnumerable<int> ids)
	{
		ProductIds = ids.Distinct().ToList();
	}
}

public record GetCharacteristicsBatchResult(
	Dictionary<int, List<ProductCharacteristicDto>> Characteristics);

public class GetCharacteristicsBatchHandler(
	IReadRepository<ProductCharacteristic, (int, string)> repository,
	IProjectionProvider<ProductCharacteristic, ProductCharacteristicDto> projection
	) : IQueryHandler<GetCharacteristicsBatchQuery, GetCharacteristicsBatchResult>
{
	public async Task<GetCharacteristicsBatchResult> Handle(
		GetCharacteristicsBatchQuery request,
		CancellationToken cancellationToken)
	{
		if (request.ProductIds.Count == 0)
			return new GetCharacteristicsBatchResult([]);

		var result = (await repository
			.Query
			.Where(x => request.ProductIds.Contains(x.ProductId))
			.Project(projection)
			.ToListAsync(cancellationToken))
			.GroupBy(x => x.ProductId)
			.ToDictionary(
				x => x.Key,
				x => x.ToList());

		return new GetCharacteristicsBatchResult(result);
	}
}
