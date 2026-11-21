using Application.Common.Dtos;
using Application.Common.Exceptions;
using Domain.CommonEnums;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Jobs.DataLoaders;
using Main.Modules.Jobs.Application.Queries.Jobs;
using MediatR;

namespace Main.Api.GraphQl.Jobs.Types;

[GraphQLName("Job")]
public sealed class GqlJob
{
	private JobDto? _dto;

	public GqlJob(Guid id) => Id = id;

	public GqlJob(JobDto dto)
	{
		_dto = dto;
		Id = dto.Id;
	}

	[GraphQLName("id")]
	public Guid Id { get; }

	[GraphQLName("isStep")]
	public async Task<bool> GetIsStepAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).IsStep;

	[GraphQLName("multiStepJobId")]
	public async Task<Guid?> GetMultiStepJobIdAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).MultiStepJobId;

	[GraphQLName("stepKey")]
	public async Task<string?> GetStepKeyAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).StepKey;

	[GraphQLName("inputInitialized")]
	public async Task<bool> GetInputInitializedAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).InputInitialized;

	[GraphQLName("systemName")]
	public async Task<string> GetSystemNameAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).SystemName;

	[GraphQLName("status")]
	public async Task<JobStatus> GetStatusAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Status;

	[GraphQLName("attempts")]
	public async Task<int> GetAttemptsAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Attempts;

	[GraphQLName("maxAttempts")]
	public async Task<int> GetMaxAttemptsAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).MaxAttempts;

	[GraphQLName("errorMessage")]
	public async Task<string?> GetErrorMessageAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).ErrorMessage;

	[GraphQLName("lockedAt")]
	public async Task<DateTime?> GetLockedAtAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).LockedAt;

	[GraphQLName("createdAt")]
	public async Task<DateTime> GetCreatedAtAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).CreatedAt;

	[GraphQLName("updatedAt")]
	public async Task<DateTime> GetUpdatedAtAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).UpdatedAt;

	[GraphQLName("createdBy")]
	public async Task<Guid?> GetCreatedByAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).CreatedBy;

	[GraphQLName("state")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<GqlJobState> GetStateAsync(ISender sender, CancellationToken cancellationToken)
		=> new(await sender.Send(new GetJobStateQuery(Id), cancellationToken));

	private async Task<JobDto> GetDtoAsync(
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ??= await loader.LoadAsync(Id, cancellationToken)
			?? throw new JobNotFoundException(Id);
}
