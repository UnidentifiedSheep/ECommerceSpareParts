using Application.Common.Interfaces.Services;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Jobs.Types;
using Main.Api.GraphQl.Jobs.Types.Inputs;
using Main.Modules.Jobs.Application.Commands.Jobs;
using MediatR;

namespace Main.Api.GraphQl.Jobs.Mutations;

public sealed class JobMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.JOBS_CREATE)]
	public async Task<GqlJob> CreateAsync(
		GqlCreateJobInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var created = await sender.Send(
			new CreateJobCommand(
				input.SystemName,
				input.InputState,
				input.MaxAttempts),
			cancellationToken);

		return new GqlJob(created.JobId);
	}

	[GraphQLName("cancel")]
	[RequireAllPermissions(PermissionCodes.JOBS_CREATE)]
	public async Task<bool> CancelAsync(
		Guid id,
		IJobService jobService,
		CancellationToken cancellationToken)
	{
		await jobService.CancelJobAsync(id, cancellationToken);
		return true;
	}
}
