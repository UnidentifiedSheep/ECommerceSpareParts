using Application.Common.Interfaces.Services;
using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Modules.Jobs.Api.GraphQl.Types;
using Main.Modules.Jobs.Api.GraphQl.Types.Inputs;
using Main.Modules.Jobs.Application.Commands.Jobs;
using MediatR;

namespace Main.Modules.Jobs.Api.GraphQl.Mutations;

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
