using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Api.GraphQl.DataLoaders;
using Main.Api.GraphQl.Types;

namespace Main.Api.GraphQl.Queries;

public sealed class CatalogueCandidateQueries
{
	[GraphQLName("byId")]
	[Lookup]
	public async Task<GqlCatalogueCandidate?> GetCandidateByIdAsync(
		ICatalogueCandidateByIdDataLoader loader,
		Guid id,
		CancellationToken ct)
	{
		var candidate = await loader.LoadAsync(id, ct);
		return candidate is null ? null : new GqlCatalogueCandidate(candidate);
	}
}
