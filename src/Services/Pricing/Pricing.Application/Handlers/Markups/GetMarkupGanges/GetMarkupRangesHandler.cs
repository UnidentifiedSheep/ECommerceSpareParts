using Application.Common.Interfaces;
using Mapster;
using Pricing.Abstractions.Dtos.Markups;
using Pricing.Abstractions.Interfaces.DbRepositories;

namespace Pricing.Application.Handlers.Markups.GetMarkupGanges;

public record GetMarkupRangesQuery(int GroupId) : IQuery<GetMarkupRangesResult>;

public record GetMarkupRangesResult(IEnumerable<MarkupRangeDto> Ranges);

public class GetMarkupRangesHandler(IMarkupRepository markupRepository)
    : IQueryHandler<GetMarkupRangesQuery, GetMarkupRangesResult>
{
    public async Task<GetMarkupRangesResult> Handle(GetMarkupRangesQuery request, CancellationToken cancellationToken)
    {
        var ranges = await markupRepository.GetMarkupRanges(request.GroupId, false, cancellationToken);
        return new GetMarkupRangesResult(ranges.Adapt<List<MarkupRangeDto>>());
    }
}