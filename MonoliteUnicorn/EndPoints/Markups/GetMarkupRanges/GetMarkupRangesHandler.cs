using Core.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Markups;
using MonoliteUnicorn.Exceptions.Markups;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Markups.GetMarkupRanges;

public record GetMarkupRangesQuery(int GroupId) : IQuery<GetMarkupRangesResult>;
public record GetMarkupRangesResult(IEnumerable<MarkupRangeDto> Ranges);

public class GetMarkupRangesHandler(DContext context) : IQueryHandler<GetMarkupRangesQuery, GetMarkupRangesResult>
{
    public async Task<GetMarkupRangesResult> Handle(GetMarkupRangesQuery request, CancellationToken cancellationToken)
    {
        var groupExists = await context.MarkupGroups
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.GroupId, cancellationToken);
        if (!groupExists)
            throw new MarkupGroupNotFoundException(request.GroupId);

        var ranges = await context.MarkupRanges
            .AsNoTracking()
            .Where(x => x.GroupId == request.GroupId)
            .ToListAsync(cancellationToken);
        return new GetMarkupRangesResult(ranges.Adapt<List<MarkupRangeDto>>());
    }
}