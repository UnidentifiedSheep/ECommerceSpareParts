using HotChocolate;
using Main.Modules.Jobs.Application.Queries.Jobs;

namespace Main.Api.GraphQl.Jobs.Types;

[GraphQLName("JobState")]
public sealed record GqlJobState(
	[property: GraphQLIgnore]
	GetJobStateResult State)
{
	[GraphQLName("state")]
	public string Value => State.State;

	[GraphQLName("inputInitialized")]
	public bool InputInitialized => State.InputInitialized;

	[GraphQLName("output")]
	public string? Output => State.Output;
}
