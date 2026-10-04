using HotChocolate;

namespace Main.Api.GraphQl.Types.Inputs.Document;

[GraphQLName("CreateGenerationRequestInput")]
public sealed record GqlCreateGenerationRequestInput
{
	[GraphQLName("documentSystemName")]
	public required string DocumentSystemName { get; init; }

	[GraphQLName("documentRequest")]
	public required string DocumentRequest { get; init; }
}
