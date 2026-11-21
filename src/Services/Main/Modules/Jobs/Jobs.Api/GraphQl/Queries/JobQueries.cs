using Application.Common.Dtos;
using Enums;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using HotChocolate;
using HotChocolate.Types.Composite;
using Main.Modules.Jobs.Api.GraphQl.DataLoaders;
using Main.Modules.Jobs.Api.GraphQl.Types;
using Main.Modules.Jobs.Api.GraphQl.Types.Inputs;
using Main.Modules.Jobs.Application.Queries.Jobs;
using Main.Modules.Jobs.Application.Queries.Jobs.GetJobs;
using MediatR;

namespace Main.Modules.Jobs.Api.GraphQl.Queries;

public sealed class JobQueries
{
	[GraphQLName("available")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<IReadOnlyList<GqlJobInfo>> GetAvailableAsync(
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetAllAvailableJobsQuery(), cancellationToken)).Jobs
			.Select(job => new GqlJobInfo(job))
			.ToArray();

	[GraphQLName("byId")]
	[Lookup]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<GqlJob?> GetByIdAsync(
		Guid id,
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
	{
		var job = await loader.LoadAsync(id, cancellationToken);
		return job is null ? null : new GqlJob(job);
	}

	[GraphQLName("byIds")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<IReadOnlyList<GqlJob>> GetByIdsAsync(
		IReadOnlyCollection<Guid> ids,
		IJobByIdDataLoader loader,
		CancellationToken cancellationToken)
		=> (await loader.LoadAsync(ids, cancellationToken))
			.OfType<JobDto>()
			.Select(job => new GqlJob(job))
			.ToArray();

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.JOBS_GET)]
	public async Task<IReadOnlyList<GqlJob>> SearchAsync(
		GqlSearchJobsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetJobsQuery(
				input.Pagination,
				input.SystemNames,
				input.Statuses,
				input.SortBy.ToSortExpressions()),
			cancellationToken);
		return result.Jobs.Select(job => new GqlJob(job)).ToArray();
	}
}
