using Application.Common.Dtos;
using Application.Common.Models;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Modules.Jobs.Api.GraphQl.DataLoaders;
using Main.Modules.Jobs.Api.GraphQl.Types;
using Main.Modules.Jobs.Api.GraphQl.Types.Inputs;
using Main.Modules.Jobs.Application.Queries.JobSchedules.GetSchedule;
using MediatR;

namespace Main.Modules.Jobs.Api.GraphQl.Queries;

public sealed class JobScheduleQueries
{
	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<GqlJobSchedule?> GetByIdAsync(
		Guid id,
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var schedule = await loader.LoadAsync(id, cancellationToken);
		return schedule is null ? null : new GqlJobSchedule(schedule);
	}

	[GraphQLName("byIds")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<IReadOnlyList<GqlJobSchedule>> GetByIdsAsync(
		IReadOnlyCollection<Guid> ids,
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(ids, cancellationToken))
			.OfType<JobScheduleDto>()
			.Select(schedule => new GqlJobSchedule(schedule))
			.ToArray();

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<IReadOnlyList<GqlJobSchedule>> SearchAsync(
		GqlSearchJobSchedulesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetScheduleQuery(
				input.JobSystemNames,
				new RangeModel<DateTime>(input.NextRunFrom, input.NextRunTo),
				input.SortBy.ToSortExpressions(),
				input.Pagination),
			cancellationToken);
		return result.Schedules.Select(schedule => new GqlJobSchedule(schedule)).ToArray();
	}
}
