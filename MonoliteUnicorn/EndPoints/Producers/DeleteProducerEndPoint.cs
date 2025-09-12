using Application.Handlers.Producers.DeleteProducer;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers;

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
            .WithTags("Producers")
            .WithDescription("Удаление производителя из бд")
            .WithDisplayName("Удаление производителя");
    }
}