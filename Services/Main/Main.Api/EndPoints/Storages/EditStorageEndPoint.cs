using Main.Application.Handlers.Storages.EditStorage;
using Carter;
using Core.Dtos.Amw.Storage;
using MediatR;

namespace Main.Api.EndPoints.Storages;

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
            .WithTags("Storages")
            .WithDescription("Редактирование полей склада")
            .WithDisplayName("Редактирование склада");
    }
}