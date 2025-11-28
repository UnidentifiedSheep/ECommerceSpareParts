using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Producers.DeleteProducer;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public class DeleteProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/producers/{id}", async (ISender sender, int id, CancellationToken cancellationToken) =>
            {
                var command = new DeleteProducerCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            }).WithTags("Producers")
            .WithDescription("Удаление производителя из бд")
            .WithDisplayName("Удаление производителя")
            .RequireAnyPermission("PRODUCERS.DELETE");
    }
}