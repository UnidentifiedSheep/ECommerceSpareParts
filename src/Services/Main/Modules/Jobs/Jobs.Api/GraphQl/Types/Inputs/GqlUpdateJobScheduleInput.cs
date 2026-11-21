using Application.Common.Dtos;
using GraphQL.Common.Types;
using HotChocolate;

namespace Main.Modules.Jobs.Api.GraphQl.Types.Inputs;

[GraphQLName("UpdateJobScheduleInput")]
public sealed record GqlUpdateJobScheduleInput
{
	[GraphQLName("scheduleId")]
	public required Guid ScheduleId { get; init; }

	[GraphQLName("name")]
	public GqlPatchField<string>? Name { get; init; }

	[GraphQLName("description")]
	public GqlPatchField<string?>? Description { get; init; }

	[GraphQLName("inputState")]
	public GqlPatchField<string>? InputState { get; init; }

	[GraphQLName("maxAttempts")]
	public GqlPatchField<int>? MaxAttempts { get; init; }

	[GraphQLName("cron")]
	public GqlPatchField<string>? Cron { get; init; }

	[GraphQLName("enabled")]
	public GqlPatchField<bool>? Enabled { get; init; }

	public PatchJobScheduleDto ToDto()
		=> new()
		{
			Name = Name,
			Description = Description,
			InputState = InputState,
			MaxAttempts = MaxAttempts,
			Cron = Cron,
			Enabled = Enabled
		};
}
