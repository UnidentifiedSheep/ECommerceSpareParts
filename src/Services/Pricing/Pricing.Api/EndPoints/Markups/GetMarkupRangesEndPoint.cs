using Api.Common.Extensions;
using Carter;
using Mapster;
using MediatR;
using Pricing.Abstractions.Dtos.Markups;
using Pricing.Application.Handlers.Markups.GetMarkupGanges;

namespace Pricing.Api.EndPoints.Markups;

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
            }).WithTags("Markups")
            .WithDescription("Получение диапазонов группы")
            .WithDisplayName("Получение диапазонов группы")
            .RequireAnyPermission("MARKUP.GET");
    }
}