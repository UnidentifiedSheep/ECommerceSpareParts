using Application.Common.Interfaces.Services;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Jobs.Types;
using Main.Api.GraphQl.Jobs.Types.Inputs;

namespace Main.Api.GraphQl.Jobs.Mutations;

public sealed class JobScheduleMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.JOBS_CREATE)]
	public async Task<GqlJobSchedule> CreateAsync(
		GqlCreateJobScheduleInput input,
		IJobScheduleService jobScheduleService,
		CancellationToken cancellationToken)
	{
		var scheduleId = await jobScheduleService.CreateScheduleAsync(input.ToDto(), cancellationToken);
		return new GqlJobSchedule(scheduleId);
	}

	[GraphQLName("update")]
	[RequireAllPermissions(PermissionCodes.JOBS_CREATE)]
	public async Task<GqlJobSchedule> UpdateAsync(
		GqlUpdateJobScheduleInput input,
		IJobScheduleService jobScheduleService,
		CancellationToken cancellationToken)
	{
		await jobScheduleService.UpdateScheduleAsync(input.ScheduleId, input.ToDto(), cancellationToken);
		return new GqlJobSchedule(input.ScheduleId);
	}

	[GraphQLName("remove")]
	[RequireAllPermissions(PermissionCodes.JOBS_CREATE)]
	public async Task<bool> RemoveAsync(
		Guid id,
		IJobScheduleService jobScheduleService,
		CancellationToken cancellationToken)
	{
		await jobScheduleService.RemoveScheduleAsync(id, cancellationToken);
		return true;
	}
}
