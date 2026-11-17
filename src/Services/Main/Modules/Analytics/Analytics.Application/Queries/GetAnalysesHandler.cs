using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;
using Main.Modules.Analytics.Abstractions.Analyses;
using Main.Modules.Analytics.Application.Dtos.Analyses;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Analytics.Application.Queries;

public sealed record GetAnalysesQuery : IQuery<GetAnalysesResult>;

public sealed record GetAnalysesResult(IReadOnlyList<AnalysisDto> Analyses);

public sealed class GetAnalysesHandler(
	INamedObjectRegistry<AnalysisNamedObject> registry,
	IContextualLocalizer localizer) : IQueryHandler<GetAnalysesQuery, GetAnalysesResult>
{
	public Task<GetAnalysesResult> Handle(GetAnalysesQuery request, CancellationToken cancellationToken)
	{
		var analyses = registry
			.All
			.Select(analysis => new AnalysisDto
			{
				SystemName = analysis.SystemName,
				Name = analysis.GetLocalizedName(localizer),
				Description = analysis.GetLocalizedDescription(localizer),
				LrtSystemName = analysis.LrtSystemName,
				ItemsChartSystemName = analysis.ItemsChartSystemName,
				SchemaVersion = analysis.SchemaVersion,
				ParametersSchema = analysis.ParametersSchema,
				SummarySchema = analysis.SummarySchema,
				SegmentsSchema = analysis.SegmentsSchema,
				DataSchema = analysis.DataSchema
			})
			.OrderBy(analysis => analysis.SystemName, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Task.FromResult(new GetAnalysesResult(analyses));
	}
}
