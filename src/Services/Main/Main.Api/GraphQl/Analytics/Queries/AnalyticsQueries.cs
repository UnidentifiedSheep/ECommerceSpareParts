using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Application.Common.Models;
using Main.Api.GraphQl.Analytics.DataLoaders;
using Main.Api.GraphQl.Analytics.Inputs;
using Main.Api.GraphQl.Analytics.Types;
using Main.Modules.Analytics.Application.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.Queries;

public sealed class AnalyticsQueries
{
	[GraphQLName("charts")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<IReadOnlyList<GqlChart>> GetChartsAsync(
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetChartsQuery(), cancellationToken)).Charts
			.Select(chart => new GqlChart(chart))
			.ToArray();

	[GraphQLName("chart")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<GqlChart?> GetChartAsync(
		string systemName,
		IChartBySystemNameDataLoader loader,
		CancellationToken cancellationToken)
	{
		var chart = await loader.LoadAsync(systemName, cancellationToken);
		return chart is null ? null : new GqlChart(chart);
	}

	[GraphQLName("runs")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<IReadOnlyList<GqlAnalyticsRun>> GetRunsAsync(
		GqlGetAnalyticsRunsInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var cursorValue = input.Cursor.Value;
		var result = await sender.Send(
			new GetAnalyticsRunsQuery(
				input.AnalysisType,
				input.IsPinned,
				input.PeriodFrom,
				input.PeriodTo,
				new Cursor<(Guid Id, DateTime CreatedAt)>(
					cursorValue is { } value ? (value.Id, value.CreatedAt) : (Guid.Empty, DateTime.MinValue),
					input.Cursor.Size)),
			cancellationToken);

		return result.Runs.Select(run => new GqlAnalyticsRun(run)).ToArray();
	}

	[GraphQLName("run")]
	[RequireAllPermissions(PermissionCodes.CHARTS_GET)]
	public async Task<GqlAnalyticsRun?> GetRunAsync(
		Guid id,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(new GetAnalyticsRunByIdQuery(id), cancellationToken);
		return result.Run is null ? null : new GqlAnalyticsRun(result.Run);
	}
}
