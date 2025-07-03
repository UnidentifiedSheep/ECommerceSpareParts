using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Amw.Markups;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Markups.GetMarkupGroups;

public record GetMarkupGroupsQuery(int Page, int ViewCount) : IQuery<GetMarkupGroupsResult>;
public record GetMarkupGroupsResult(IEnumerable<MarkupGroupDto> Groups);

public class GetMarkupGroupsValidation : AbstractValidator<GetMarkupGroupsQuery>
{
    public GetMarkupGroupsValidation()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Страница не может быть меньше 0");

        RuleFor(query => query.ViewCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Количество элементов должно быть от 1 до 100");
    }
}

public class GetMarkupGroupsHandler(DContext context) :IQueryHandler<GetMarkupGroupsQuery, GetMarkupGroupsResult>
{
    public async Task<GetMarkupGroupsResult> Handle(GetMarkupGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await context.MarkupGroups
            .AsNoTracking()
            .Include(x => x.Currency)
            .Skip(request.Page * request.ViewCount)
            .Take(request.ViewCount)
            .ToListAsync(cancellationToken);
        return new GetMarkupGroupsResult(groups.Adapt<List<MarkupGroupDto>>());
    }
}