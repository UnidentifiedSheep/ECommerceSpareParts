using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Storage;

namespace MonoliteUnicorn.EndPoints.Storages.EditStorage;

public record EditStorageRequest(PatchStorageDto EditStorage);

public class EditStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/{storageName}",
            async (ISender sender, string storageName, EditStorageRequest request, CancellationToken token) =>
            {
                var command = new EditStorageCommand(storageName, request.EditStorage);
                await sender.Send(command, token);
                return Results.NoContent();
            }).RequireAuthorization("AMW")
            .WithGroup("Storages")
            .WithDescription("Редактирование полей склада")
            .WithDisplayName("Редактирование склада");
    }
}