using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Markups;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Markups.GetMarkupGanges;

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