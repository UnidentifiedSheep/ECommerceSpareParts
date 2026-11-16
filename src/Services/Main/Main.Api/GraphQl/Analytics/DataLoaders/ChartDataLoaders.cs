using GreenDonut;
using Main.Modules.Analytics.Application.Dtos.Charts;
using Main.Modules.Analytics.Application.Queries;
using MediatR;

namespace Main.Api.GraphQl.Analytics.DataLoaders;

public static class ChartDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<string, ChartDto>> GetChartBySystemNameAsync(
		IReadOnlyList<string> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetChartsQuery(), cancellationToken)).Charts
			.ToDictionary(chart => chart.SystemName, StringComparer.OrdinalIgnoreCase);
}
