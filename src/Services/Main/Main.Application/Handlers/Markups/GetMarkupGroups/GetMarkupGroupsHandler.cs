using Application.Common.Interfaces;
using Core.Dtos.Amw.Markups;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Mapster;

namespace Main.Application.Handlers.Markups.GetMarkupGroups;

public record GetMarkupGroupsQuery(PaginationModel Pagination) : IQuery<GetMarkupGroupsResult>;

public record GetMarkupGroupsResult(IEnumerable<MarkupGroupDto> Groups);

public class GetMarkupGroupsHandler(IMarkupRepository markupRepository)
    : IQueryHandler<GetMarkupGroupsQuery, GetMarkupGroupsResult>
{
    public async Task<GetMarkupGroupsResult> Handle(GetMarkupGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await markupRepository.GetMarkupGroups(request.Pagination.Page, request.Pagination.Size,
            false, cancellationToken);
        return new GetMarkupGroupsResult(groups.Adapt<List<MarkupGroupDto>>());
    }
}