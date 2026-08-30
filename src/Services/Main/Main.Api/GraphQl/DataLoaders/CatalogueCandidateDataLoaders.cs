using GreenDonut;
using Main.Application.Dtos.Product.Enrichment;
using Main.Application.Handlers.ProductEnrichment;
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
}
