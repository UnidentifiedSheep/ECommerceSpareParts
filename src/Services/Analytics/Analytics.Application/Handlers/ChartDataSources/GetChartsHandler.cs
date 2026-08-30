using Analytics.Application.Dtos.Charts;
using Analytics.Application.NamedObjects.ChartDataSources;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.ChartDataSources;

public sealed record GetChartsQuery : IQuery<GetChartsResult>;

public sealed record GetChartsResult(IReadOnlyList<ChartDto> Charts);

public sealed class GetChartsHandler(
	INamedObjectRegistry<ChartDataSourceNamedObject> registry,
	IContextualStringLocalizer localizer) : IQueryHandler<GetChartsQuery, GetChartsResult>
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
