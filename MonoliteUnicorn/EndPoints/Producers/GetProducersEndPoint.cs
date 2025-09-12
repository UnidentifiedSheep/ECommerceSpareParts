using Application.Handlers.Producers.GetProducers;
using Carter;
using Core.Dtos.Anonymous.Producers;
using Core.Models;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers;
public record GetProducersResponse(IEnumerable<ProducerDto> Producers);

public class GetProducersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers/", async (ISender sender, string? searchTerm, int page, int viewCount) =>
        {
            var query = new GetProducersQuery(searchTerm,  new PaginationModel(page, viewCount));
            var result = await sender.Send(query);
            return Results.Ok(result.Adapt<GetProducersResponse>());
        }).WithTags("Producers")
        .WithDescription("Получение производителей по ключевому слову либо просто списком")
        .Produces<GetProducersResponse>();
    }
}