using Carter;
using Core.Models;
using Main.Application.Handlers.Producers.GetProducers;
using Main.Core.Dtos.Anonymous.Producers;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record GetProducersResponse(IEnumerable<ProducerDto> Producers);

public class GetProducersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers", async (ISender sender, string? searchTerm, int page, int limit) =>
            {
                var query = new GetProducersQuery(searchTerm, new PaginationModel(page, limit));
                var result = await sender.Send(query);
                return Results.Ok(result.Adapt<GetProducersResponse>());
            }).WithTags("Producers")
            .WithDescription("Получение производителей по ключевому слову либо просто списком")
            .Produces<GetProducersResponse>();
    }
}