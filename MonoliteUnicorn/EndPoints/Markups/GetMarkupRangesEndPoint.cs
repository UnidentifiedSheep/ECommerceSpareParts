using Application.Handlers.Markups.GetMarkupGanges;
using Carter;
using Core.Dtos.Amw.Markups;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Markups;

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
        .WithTags("Markups")
        .WithDescription("Получение диапазонов группы")
        .WithDisplayName("Получение диапазонов группы");
    }
}