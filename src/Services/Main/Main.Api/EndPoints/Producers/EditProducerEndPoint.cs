using Carter;
using Main.Application.Handlers.Producers.EditProducer;
using Main.Core.Dtos.Amw.Producers;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record EditProducerRequest(PatchProducerDto EditProducer);

public class EditProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/producers/{producerId}",
                async (ISender sender, int producerId, EditProducerRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new EditProducerCommand(producerId, request.EditProducer);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                }).WithTags("Producers")
                .WithDescription("Редактирование производителя")
                .WithDisplayName("Редактирование производителя");
    }
}