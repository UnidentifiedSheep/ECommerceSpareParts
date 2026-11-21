using Application.Common.Dtos;
using Application.Common.Exceptions;
using HotChocolate;
using Main.Api.GraphQl.Jobs.DataLoaders;

namespace Main.Api.GraphQl.Jobs.Types;

[GraphQLName("JobSchedule")]
public sealed class GqlJobSchedule
{
	private JobScheduleDto? _dto;

	public GqlJobSchedule(Guid id) => Id = id;

	public GqlJobSchedule(JobScheduleDto dto)
	{
		_dto = dto;
		Id = dto.Id;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("name")]
	public async Task<string> GetNameAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Name;

	[GraphQLName("description")]
	public async Task<string?> GetDescriptionAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Description;

	[GraphQLName("jobSystemName")]
	public async Task<string> GetJobSystemNameAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).JobSystemName;

	[GraphQLName("inputState")]
	public async Task<string> GetInputStateAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).InputState;

	[GraphQLName("maxAttempts")]
	public async Task<int> GetMaxAttemptsAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).MaxAttempts;

	[GraphQLName("cron")]
	public async Task<string> GetCronAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Cron;

	[GraphQLName("localizedCron")]
	public async Task<string> GetLocalizedCronAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).LocalizedCron;

	[GraphQLName("lastQueuedAt")]
	public async Task<DateTime?> GetLastQueuedAtAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).LastQueuedAt;

	[GraphQLName("nextRunAt")]
	public async Task<DateTime?> GetNextRunAtAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).NextRunAt;

	[GraphQLName("enabled")]
	public async Task<bool> GetEnabledAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Enabled;

	private async Task<JobScheduleDto> GetDtoAsync(
		IJobScheduleByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ??= await loader.LoadAsync(Id, cancellationToken)
			?? throw new JobScheduleNotFoundException(Id);
}
