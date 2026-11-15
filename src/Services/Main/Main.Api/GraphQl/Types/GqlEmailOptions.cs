using HotChocolate;
using Main.Application.Models.Options;

namespace Main.Api.GraphQl.Types;

[GraphQLName("EmailOptions")]
public sealed record GqlEmailOptions(
	[property: GraphQLIgnore]
	UserEmailOptions Options)
{
	[GraphQLName("maxEmailCount")]
	public int MaxEmailCount => Options.MaxEmailCount;

	[GraphQLName("minEmailCount")]
	public int MinEmailCount => Options.MinEmailCount;
}
