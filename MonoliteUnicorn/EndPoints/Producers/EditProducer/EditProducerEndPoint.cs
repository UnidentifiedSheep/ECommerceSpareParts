using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Producers;

namespace MonoliteUnicorn.EndPoints.Producers.EditProducer;

public record EditProducerRequest(PatchProducerDto EditProducer);

public class EditProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/producers/{producerId}",
            async (ISender sender, int producerId, EditProducerRequest request, CancellationToken cancellationToken) => 
            {
                var command = new EditProducerCommand(producerId, request.EditProducer);
                await sender.Send(command, cancellationToken);
                return Results.NoContent(); 
            }).RequireAuthorization("AMW")
            .WithGroup("Producers")
            .WithDescription("Редактирование производителя")
            .WithDisplayName("Редактирование производителя");
    }
}