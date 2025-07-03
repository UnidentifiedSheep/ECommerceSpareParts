using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Markups;

namespace MonoliteUnicorn.EndPoints.Markups.GetMarkupRanges;

public record GetMarkupRangesResponse(IEnumerable<MarkupRangeDto> Ranges);

public class GetMarkupRangesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/markups/{groupId}", async (ISender sender, int groupId, CancellationToken token) =>
        {
            var query = new GetMarkupRangesQuery(groupId);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetMarkupRangesResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AM")
        .WithGroup("Markups")
        .WithDescription("Получение диапазонов группы")
        .WithDisplayName("Получение диапазонов группы");
    }
}