using GreenDonut;
using Main.Modules.Product.Application.Dtos.Product.Enrichment;
using Main.Modules.Product.Application.Queries.ProductEnrichment;
using MediatR;

namespace Main.Api.GraphQl.DataLoaders;

public static class CatalogueCandidateDataLoaders
{
	[DataLoader]
	public static async Task<Dictionary<Guid, CatalogueCandidateReviewDto>> GetCatalogueCandidateByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetCatalogueCandidatesByIdsQuery(keys), cancellationToken);

		return result.Candidates.ToDictionary(x => x.Id, x => x);
	}

	[DataLoader]
	public static async Task<Dictionary<int, List<SupplierProductDto>>> GetSupplierProductCrossesByIdAsync(
		IReadOnlyList<int> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(
			new GetSupplierProductCrossesBatchQuery(keys),
			cancellationToken)).Crosses;

	[DataLoader]
	public static async Task<Dictionary<Guid, GetCatalogueCandidateCrossesResultItem>>
		GetCandidateCrossesByIdAsync(
			IReadOnlyList<Guid> keys,
			ISender sender,
			CancellationToken cancellationToken)
		=> (await sender.Send(
			new GetCatalogueCandidateCrossesQuery(keys),
			cancellationToken)).Items;
}
