using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.CatalogueCandidate;

[GraphQLName("CandidateToCatalogueInput")]
public record GqlCandidateToCatalogueInput
{
	[GraphQLName("id")]
	public required Guid Id { get; init; }

	[GraphQLName("selectedName")]
	public string? SelectedName { get; init; }
}
