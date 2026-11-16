using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;
using Main.Modules.Analytics.Abstractions.Charts;
using Main.Modules.Analytics.Application.Dtos.Charts;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Analytics.Application.Queries;

public sealed record GetChartsQuery : IQuery<GetChartsResult>;

public sealed record GetChartsResult(IReadOnlyList<ChartDto> Charts);

public sealed class GetChartsHandler(
	INamedObjectRegistry<ChartDataSourceNamedObject> registry,
	IContextualLocalizer localizer) : IQueryHandler<GetChartsQuery, GetChartsResult>
{
	public Task<GetChartsResult> Handle(GetChartsQuery request, CancellationToken cancellationToken)
	{
		var charts = registry
			.All
			.Select(chart => new ChartDto
			{
				SystemName = chart.SystemName,
				Name = chart.GetLocalizedName(localizer),
				Description = chart.GetLocalizedDescription(localizer),
				QueryInputSchema = chart.QueryInputSchema,
				DataPointSchema = chart.DataPointSchema
			})
			.OrderBy(chart => chart.SystemName, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Task.FromResult(new GetChartsResult(charts));
	}
}
