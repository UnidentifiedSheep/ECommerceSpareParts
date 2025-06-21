using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers.DeleteProducer;

public class DeleteProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/producers/{id}", async (ISender sender, int id, CancellationToken cancellationToken) =>
            {
                var command = new DeleteProducerCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            }).RequireAuthorization("AMW")
            .WithGroup("Producers")
            .WithDescription("Удаление производителя из бд")
            .WithDisplayName("Удаление производителя");
    }
}