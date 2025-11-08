using Carter;
using Core.Models;
using Main.Application.Handlers.Markups.GetMarkupGroups;
using Main.Core.Dtos.Amw.Markups;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Markups;

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
            .WithDisplayName("Получение групп наценок");
    }
}