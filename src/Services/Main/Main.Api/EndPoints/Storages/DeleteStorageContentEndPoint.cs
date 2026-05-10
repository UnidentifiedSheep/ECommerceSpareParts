using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public class DeleteStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/content/{contentId}", async (
                ISender sender,
                int contentId,
                uint rowVersion,
                CancellationToken cancellationToken) =>
            {
                var command = new SetToZeroContentCommand(contentId, rowVersion);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Полное удаление позиции со склада по его Id")
            .WithDisplayName("Удаление позиции со склада")
            .RequireAnyPermission("STORAGES.CONTENT.DELETE");
    }
}