using Application.Handlers.Markups.GetMarkupGroups;
using Carter;
using Core.Dtos.Amw.Markups;
using Core.Models;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Markups;

public record GetMarkupGroupsResponse(IEnumerable<MarkupGroupDto> Groups);

public class GetMarkupGroupsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/markups/", async (ISender sender, int page, int viewCount, CancellationToken cancellationToken) =>
        {
            var query = new GetMarkupGroupsQuery(new PaginationModel(page, viewCount));
            var result = await sender.Send(query, cancellationToken);
            var response = result.Adapt<GetMarkupGroupsResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AM")
        .WithTags("Markups")
        .WithDescription("Получение групп наценок")
        .WithDisplayName("Получение групп наценок");
    }
}