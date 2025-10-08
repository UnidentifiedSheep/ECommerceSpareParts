using Main.Application.Handlers.Producers.AddOtherName;
using Carter;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record AddOtherNameToProducerRequest(string OtherName, string? WhereUsed);

public class AddOtherNameToProducerEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/producers/{producerId}/names", async (ISender sender, int producerId,
                AddOtherNameToProducerRequest request, CancellationToken token) =>
            {
                var command = new AddOtherNameCommand(producerId, request.OtherName, request.WhereUsed);
                await sender.Send(command, token);
                return Results.Ok();
            }).WithTags("Producers")
            .RequireAuthorization("AMW")
            .WithDisplayName("Добавление дополнительного имени")
            .WithDescription("Добавление дополнительного имени к производителю");
    }
}