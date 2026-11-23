using HotChocolate;

namespace Main.Api.GraphQl.Jobs.Types.Inputs;

[GraphQLName("CreateJobInput")]
public sealed record GqlCreateJobInput
{
	[GraphQLName("systemName")]
	public required string SystemName { get; init; }

	[GraphQLName("inputState")]
	public required string InputState { get; init; }

	[GraphQLName("maxAttempts")]
	public int MaxAttempts { get; init; } = 3;
}
