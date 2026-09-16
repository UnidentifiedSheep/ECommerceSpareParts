using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment;

public record GetCatalogueCandidateCrossesQuery : IQuery<GetCatalogueCandidateCrossesResult>
{
	public IReadOnlySet<Guid> Ids { get; }

	public GetCatalogueCandidateCrossesQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.ToHashSet();
	}
}

public record GetCatalogueCandidateCrossesResultItem(
	IReadOnlyList<CatalogueCandidateReviewDto> MappedCrosses,
	IReadOnlyList<SupplierProductDto> NotMappedCrosses);

public record GetCatalogueCandidateCrossesResult(
	Dictionary<Guid, GetCatalogueCandidateCrossesResultItem> Items);

public class GetCatalogueCandidateCrossesHandler(
	IReadRepository<CatalogueCandidate, Guid> candidateRepository,
	IReadRepository<SupplierProductCross, SupplierProductCrossKey> crossRepository,
	IReadRepository<SupplierProduct, int> supplierProductRepository,
	IProjectionProvider<CatalogueCandidate, CatalogueCandidateReviewDto> candidateProjection,
	IProjectionProvider<SupplierProduct, SupplierProductDto> supplierProductProjection
	) : IQueryHandler<GetCatalogueCandidateCrossesQuery, GetCatalogueCandidateCrossesResult>
{
	public async Task<GetCatalogueCandidateCrossesResult> Handle(
		GetCatalogueCandidateCrossesQuery request,
		CancellationToken cancellationToken)
	{
		var crossesByCandidate =
			await GetCrossesByCandidate(request.Ids, cancellationToken);

		var crossProducts =
			await GetCrossProducts(crossesByCandidate, cancellationToken);

		return new GetCatalogueCandidateCrossesResult(
			BuildResultItems(crossesByCandidate, crossProducts));
	}

	private async Task<IReadOnlyList<CandidateCrosses>> GetCrossesByCandidate(
		IReadOnlySet<Guid> candidateIds,
		CancellationToken cancellationToken)
	{
		var candidateProducts =
			from candidate in candidateRepository.Query
			where candidateIds.Contains(candidate.Id)
			from supplierProduct in candidate.SupplierProducts
			select new
			{
				CandidateId = candidate.Id,
				SupplierProductId = supplierProduct.Id
			};

		var directCrosses =
			from candidateProduct in candidateProducts
			join cross in crossRepository.Query
				on candidateProduct.SupplierProductId equals cross.LeftId
			select new
			{
				candidateProduct.CandidateId,
				SupplierProductId = cross.RightId
			};

		var reverseCrosses =
			from candidateProduct in candidateProducts
			join cross in crossRepository.Query
				on candidateProduct.SupplierProductId equals cross.RightId
			select new
			{
				candidateProduct.CandidateId,
				SupplierProductId = cross.LeftId
			};

		return (await directCrosses
				.Concat(reverseCrosses)
				.ToListAsync(cancellationToken))
			.GroupBy(
				x => x.CandidateId,
				x => x.SupplierProductId)
			.Select(x => new CandidateCrosses(x.Key, x.Distinct().ToList()))
			.ToList();
	}

	private async Task<IReadOnlyDictionary<int, CrossProduct>> GetCrossProducts(
		IReadOnlyList<CandidateCrosses> crossesByCandidate,
		CancellationToken cancellationToken)
	{
		var supplierProductIds = crossesByCandidate
			.SelectMany(x => x.SupplierProductIds)
			.Distinct()
			.ToList();

		var crossProducts = await supplierProductRepository
			.Query
			.AsExpandable()
			.Where(x => supplierProductIds.Contains(x.Id))
			.Select(x => new
			{
				x.Id,
				Candidate = x.CatalogueCandidate == null
					? null
					: candidateProjection.Projection.Invoke(x.CatalogueCandidate),
				SupplierProduct = x.CatalogueCandidate == null
					? supplierProductProjection.Projection.Invoke(x)
					: null
			})
			.ToListAsync(cancellationToken);

		return crossProducts.ToDictionary(
			x => x.Id,
			x => new CrossProduct(x.Candidate, x.SupplierProduct));
	}

	private static Dictionary<Guid, GetCatalogueCandidateCrossesResultItem> BuildResultItems(
		IReadOnlyList<CandidateCrosses> crossesByCandidate,
		IReadOnlyDictionary<int, CrossProduct> crossProducts)
	{
		var result = new Dictionary<Guid, GetCatalogueCandidateCrossesResultItem>();

		foreach (var candidateCrosses in crossesByCandidate)
		{
			var mapped = new List<CatalogueCandidateReviewDto>();
			var notMapped = new List<SupplierProductDto>();

			foreach (var supplierProductId in candidateCrosses.SupplierProductIds)
			{
				if (!crossProducts.TryGetValue(supplierProductId, out var crossProduct))
					continue;

				if (crossProduct.Candidate != null)
					mapped.Add(crossProduct.Candidate);
				else
					notMapped.Add(crossProduct.SupplierProduct!);
			}

			result[candidateCrosses.CandidateId] =
				new GetCatalogueCandidateCrossesResultItem(mapped, notMapped);
		}

		return result;
	}

	private sealed record CandidateCrosses(Guid CandidateId, IReadOnlyList<int> SupplierProductIds);

	private sealed record CrossProduct(
		CatalogueCandidateReviewDto? Candidate,
		SupplierProductDto? SupplierProduct);
}
