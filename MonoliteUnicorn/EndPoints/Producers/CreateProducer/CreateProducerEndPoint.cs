using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Producers;

namespace MonoliteUnicorn.EndPoints.Producers.CreateProducer;

public record CreateProducerRequest(AmwNewProducerDto NewProducer);

public class CreateProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/producers", async (ISender sender, CreateProducerRequest request, CancellationToken token) =>
            {
                var command = request.Adapt<CreateProducerCommand>();
                var result = await sender.Send(command, token);
                return Results.Created("/producers", result.ProducerId);
            }).RequireAuthorization("AMW")
        .WithGroup("Producers")
        .WithDescription("Добавление новых производителей в бд")
        .WithDisplayName("Добавление производителей");
    }
}