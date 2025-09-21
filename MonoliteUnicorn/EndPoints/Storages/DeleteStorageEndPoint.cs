using Application.Handlers.Storages.DeleteStorage;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Storages;

public class DeleteStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/{StorageName}", async (ISender sender, string storageName, CancellationToken cancellationToken) =>
            {
                var command = new DeleteStorageCommand(storageName);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).RequireAuthorization("AM")
        .WithTags("Storages")
        .WithDescription("Полное удаление склада по его имени")
        .WithDisplayName("Удаление склада");
    }
}