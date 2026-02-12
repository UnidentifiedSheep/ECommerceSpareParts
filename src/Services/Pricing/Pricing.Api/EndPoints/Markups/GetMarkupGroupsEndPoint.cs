using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Mapster;
using MediatR;
using Pricing.Abstractions.Dtos.Markups;
using Pricing.Application.Handlers.Markups.GetMarkupGroups;

namespace Pricing.Api.EndPoints.Markups;

public record GetMarkupGroupsResponse(IEnumerable<MarkupGroupDto> Groups);

public class GetMarkupGroupsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/markups/", async (ISender sender, int page, int limit, CancellationToken cancellationToken) =>
            {
                var query = new GetMarkupGroupsQuery(new PaginationModel(page, limit));
                var result = await sender.Send(query, cancellationToken);
                var response = result.Adapt<GetMarkupGroupsResponse>();
                return Results.Ok(response);
            }).WithTags("Markups")
            .WithDescription("Получение групп наценок")
            .WithDisplayName("Получение групп наценок")
            .RequireAnyPermission("MARKUP.GET");
    }
}