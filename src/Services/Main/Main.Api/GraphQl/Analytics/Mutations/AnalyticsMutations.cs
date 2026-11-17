using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Analytics.Types;
using Main.Modules.Analytics.Application.Commands;
using Main.Modules.Analytics.Application.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.Mutations;

public sealed class AnalyticsMutations
{
	[GraphQLName("setRunPinned")]
	[RequireAllPermissions(PermissionCodes.ANALYTICS_RUNS_EDIT)]
	public async Task<GqlAnalyticsRun> SetRunPinnedAsync(
		Guid runId,
		bool isPinned,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new SetAnalyticsRunPinnedCommand(runId, isPinned), cancellationToken);
		var result = await sender.Send(new GetAnalyticsRunByIdQuery(runId), cancellationToken);
		return new GqlAnalyticsRun(result.Run!);
	}
}
