using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers.AddOtherNamesToProducer;

public record AddOtherNameToProducerRequest(string OtherName, string? WhereUsed);

public class AddOtherNameToProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/producers/{producerId}/names", async (ISender sender, int producerId, 
                AddOtherNameToProducerRequest request, CancellationToken token) =>
            {
                var command = new AddOtherNameToProducerCommand(producerId, request.OtherName, request.WhereUsed);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithGroup("Producers")
            .RequireAuthorization("AMW")
            .WithDisplayName("Добавление дополнительного имени")
            .WithDescription("Добавление дополнительного имени к производителю");
    }
}