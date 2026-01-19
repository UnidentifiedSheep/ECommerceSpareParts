using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.Storages.EditStorage;
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
                }).WithTags("Storages")
            .WithDescription("Редактирование полей склада")
            .WithDisplayName("Редактирование склада")
            .RequireAnyPermission("STORAGES.EDIT");
    }
}