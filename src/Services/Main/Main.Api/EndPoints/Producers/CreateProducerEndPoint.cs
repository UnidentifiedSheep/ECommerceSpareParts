using Carter;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Core.Dtos.Amw.Producers;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record CreateProducerRequest(NewProducerDto NewProducer);

public record CreateProducerResponse(int Id);

public class CreateProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/producers", async (ISender sender, CreateProducerRequest request, CancellationToken token) =>
            {
                var command = request.Adapt<CreateProducerCommand>();
                var result = await sender.Send(command, token);
                return Results.Created("/producers", new CreateProducerResponse(result.ProducerId));
            }).WithTags("Producers")
            .WithDescription("Добавление новых производителей в бд")
            .WithDisplayName("Добавление производителей");
    }
}