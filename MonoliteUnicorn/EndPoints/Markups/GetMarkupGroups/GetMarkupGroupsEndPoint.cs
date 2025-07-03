using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Markups;

namespace MonoliteUnicorn.EndPoints.Markups.GetMarkupGroups;

public record GetMarkupGroupsResponse(IEnumerable<MarkupGroupDto> Groups);

public class GetMarkupGroupsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/markups/", async (ISender sender, int page, int viewCount, CancellationToken cancellationToken) =>
        {
            var query = new GetMarkupGroupsQuery(page, viewCount);
            var result = await sender.Send(query, cancellationToken);
            var response = result.Adapt<GetMarkupGroupsResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AM")
        .WithGroup("Markups")
        .WithDescription("Получение групп наценок")
        .WithDisplayName("Получение групп наценок");
    }
}