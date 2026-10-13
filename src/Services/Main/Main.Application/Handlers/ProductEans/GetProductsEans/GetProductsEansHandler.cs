using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEans.GetProductsEans;

public record GetProductsEansQuery : IQuery<GetProductsEansResult>
{
	public IReadOnlyList<int> ProductIds { get; }

	public GetProductsEansQuery(IEnumerable<int> productIds)
	{
		ProductIds = productIds.Distinct().ToArray();
	}
}

public record GetProductsEansResult(IReadOnlyDictionary<int, IReadOnlyList<string>> Eans);

public class GetProductsEansHandler(
	IReadRepository<ProductEan, ProductEanKey> repository)
	: IQueryHandler<GetProductsEansQuery, GetProductsEansResult>
{
	public async Task<GetProductsEansResult> Handle(
		GetProductsEansQuery request,
		CancellationToken cancellationToken)
	{
		if (request.ProductIds.Count == 0)
			return new GetProductsEansResult(new Dictionary<int, IReadOnlyList<string>>());

		var eans = await repository.Query
			.Where(x => request.ProductIds.Contains(x.ProductId))
			.OrderBy(x => x.ProductId)
			.ThenBy(x => x.Ean)
			.Select(x => new { x.ProductId, x.Ean })
			.ToListAsync(cancellationToken);

		return new GetProductsEansResult(eans
			.GroupBy(x => x.ProductId)
			.ToDictionary(
				x => x.Key,
				IReadOnlyList<string> (x)
					=> x.Select(ean => ean.Ean).ToArray()));
	}
}
