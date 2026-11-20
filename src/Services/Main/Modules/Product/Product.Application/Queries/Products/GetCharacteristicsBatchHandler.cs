using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Queries.Products;

public record GetCharacteristicsBatchQuery : IQuery<GetCharacteristicsBatchResult>
{

	public GetCharacteristicsBatchQuery(IEnumerable<int> ids) => ProductIds = ids.Distinct().ToList();
	public IReadOnlyList<int> ProductIds { get; }
}

public record GetCharacteristicsBatchResult(Dictionary<int, List<ProductCharacteristicDto>> Characteristics);

public class GetCharacteristicsBatchHandler(
	IReadRepository<ProductCharacteristic, (int, int)> repository,
	IProjectionProvider<ProductCharacteristic, ProductCharacteristicDto> projection)
	: IQueryHandler<GetCharacteristicsBatchQuery, GetCharacteristicsBatchResult>
{
	public async Task<GetCharacteristicsBatchResult> Handle(
		GetCharacteristicsBatchQuery request,
		CancellationToken cancellationToken)
	{
		if (request.ProductIds.Count == 0)
			return new GetCharacteristicsBatchResult([]);

		var result =
			(await repository
				.Query
				.Where(x => request.ProductIds.Contains(x.ProductId))
				.Project(projection)
				.ToListAsync(cancellationToken))
			.GroupBy(x => x.ProductId)
			.ToDictionary(x => x.Key, x => x.ToList());

		return new GetCharacteristicsBatchResult(result);
	}
}
