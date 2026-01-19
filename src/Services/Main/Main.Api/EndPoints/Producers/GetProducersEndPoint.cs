using Carter;
using Core.Models;
using Main.Application.Handlers.Producers.GetProducerById;
using Main.Application.Handlers.Producers.GetProducers;
using Main.Abstractions.Dtos.Anonymous.Producers;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record GetProducersResponse(IEnumerable<ProducerDto> Producers);
public record GetProducerByIdResponse(ProducerDto Producer);

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
        
        app.MapGet("/producers/{id}", async (ISender sender, int id) =>
            {
                var query = new GetProducerByIdQuery(id);
                var result = await sender.Send(query);
                return Results.Ok(result.Adapt<GetProducerByIdResponse>());
            }).WithTags("Producers")
            .WithDisplayName("Получение производителя по Id")
            .WithDescription("Получение производителя по Id")
            .Produces<GetProducerByIdResponse>()
            .ProducesProblem(404);
    }
}