using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Dtos.Runs;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Analytics.Application.Queries;

public sealed record GetAnalyticsRunByIdQuery(Guid RunId) : IQuery<GetAnalyticsRunByIdResult>;

public sealed record GetAnalyticsRunByIdResult(AnalyticsRunDto? Run);

public sealed class GetAnalyticsRunByIdHandler(
	IReadRepository<AnalyticsRun, Guid> repository,
	IProjectionProvider<AnalyticsRun, AnalyticsRunDto> projection)
	: IQueryHandler<GetAnalyticsRunByIdQuery, GetAnalyticsRunByIdResult>
{
	public async Task<GetAnalyticsRunByIdResult> Handle(
		GetAnalyticsRunByIdQuery request,
		CancellationToken cancellationToken)
	{
		var run = await repository.Query
			.Where(item => item.Id == request.RunId)
			.Project(projection)
			.FirstOrDefaultAsync(cancellationToken);

		return new GetAnalyticsRunByIdResult(run);
	}
}
