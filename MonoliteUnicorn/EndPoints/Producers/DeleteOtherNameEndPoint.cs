using Application.Handlers.Producers.DeleteOtherName;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers;

public class DeleteOtherNameEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/producers/{producerId}/names/{otherName}", 
            async (ISender sender, int producerId, string otherName, string? usage, CancellationToken cancellationToken) =>
        {
            var command = new DeleteOtherNameCommand(producerId, otherName, usage);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithTags("Producers")
            .RequireAuthorization("AMW")
            .WithDisplayName("Удаление дополнительного имени")
            .WithDescription("Удаление дополнительного имени у производителю");
    }
}