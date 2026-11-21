using Application.Common.Dtos;
using GreenDonut;
using Main.Modules.Jobs.Application.Queries.Jobs;
using Main.Modules.Jobs.Application.Queries.JobSchedules;
using MediatR;

namespace Main.Api.GraphQl.Jobs.DataLoaders;

public static class JobDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, JobDto>> GetJobByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetJobsByIdsQuery(keys), cancellationToken)).Jobs;

	[DataLoader]
	public static async Task<IReadOnlyDictionary<Guid, JobScheduleDto>> GetJobScheduleByIdAsync(
		IReadOnlyList<Guid> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetSchedulesByIdsQuery(keys), cancellationToken)).Schedules;
}
