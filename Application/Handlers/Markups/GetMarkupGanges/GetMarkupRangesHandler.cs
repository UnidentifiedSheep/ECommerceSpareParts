using Application.Interfaces;
using Core.Dtos.Amw.Markups;
using Core.Interfaces.DbRepositories;
using Exceptions.Exceptions.Markups;
using Mapster;

namespace Application.Handlers.Markups.GetMarkupGanges;

public record GetMarkupRangesQuery(int GroupId) : IQuery<GetMarkupRangesResult>;

public record GetMarkupRangesResult(IEnumerable<MarkupRangeDto> Ranges);

public class GetMarkupRangesHandler(IMarkupRepository markupRepository)
    : IQueryHandler<GetMarkupRangesQuery, GetMarkupRangesResult>
{
    public async Task<GetMarkupRangesResult> Handle(GetMarkupRangesQuery request, CancellationToken cancellationToken)
    {
        var groupId = request.GroupId;
        await ValidateData(groupId, cancellationToken);
        var ranges = await markupRepository.GetMarkupRanges(groupId, false, cancellationToken);
        return new GetMarkupRangesResult(ranges.Adapt<List<MarkupRangeDto>>());
    }

    private async Task ValidateData(int groupId, CancellationToken cancellationToken = default)
    {
        if (!await markupRepository.MarkupExists(groupId, cancellationToken))
            throw new MarkupGroupNotFoundException(groupId);
    }
}