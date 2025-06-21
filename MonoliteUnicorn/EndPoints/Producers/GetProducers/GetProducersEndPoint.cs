using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Producers;

namespace MonoliteUnicorn.EndPoints.Producers.GetProducers;
public record GetProducersResponse(IEnumerable<ProducerDto> Producers);

public class GetProducersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers/", async (ISender sender, string? searchTerm, int? page, int? viewCount) =>
        {
            var query = new GetProducersQuery(searchTerm,  viewCount ?? 24, page ?? 0);
            var result = await sender.Send(query);
            return Results.Ok(result.Adapt<GetProducersResponse>());
        }).WithGroup("Producers")
        .WithDescription("Получение производителей по ключевому слову либо просто списком")
        .Produces<GetProducersResponse>();
    }
}