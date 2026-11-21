using Application.Common.Dtos;
using HotChocolate;

namespace Main.Api.GraphQl.Jobs.Types.Inputs;

[GraphQLName("CreateJobScheduleInput")]
public sealed record GqlCreateJobScheduleInput
{
	[GraphQLName("name")]
	public required string Name { get; init; }

	[GraphQLName("description")]
	public string? Description { get; init; }

	[GraphQLName("jobSystemName")]
	public required string JobSystemName { get; init; }

	[GraphQLName("inputState")]
	public required string InputState { get; init; }

	[GraphQLName("maxAttempts")]
	public int MaxAttempts { get; init; } = 3;

	[GraphQLName("cron")]
	public required string Cron { get; init; }

	[GraphQLName("enabled")]
	public required bool Enabled { get; init; }

	public NewJobScheduleDto ToDto()
		=> new()
		{
			Name = Name,
			Description = Description,
			JobSystemName = JobSystemName,
			InputState = InputState,
			MaxAttempts = MaxAttempts,
			Cron = Cron,
			Enabled = Enabled
		};
}
