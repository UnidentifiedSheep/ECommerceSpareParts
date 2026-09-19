using HotChocolate;
using Main.Enums.Products;

namespace Main.Api.GraphQl.Types.Inputs.CatalogueCandidate;

[GraphQLName("MaxCandidateCrossesInput")]
public record GqlMaxCandidateCrossesInput
{
	[GraphQLName("candidateId")]
	public required Guid CandidateId { get; init; }

	[GraphQLName("crossCandidateIds")]
	public required IReadOnlyCollection<Guid> CrossCandidateIds { get; init; }

	[GraphQLName("linkageType")]
	public required ProductLinkageType LinkageType { get; init; }
}
