using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.EditContent;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record EditStorageContentRequest(
    Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields);

public class EditStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/content", async (
                ISender sender,
                EditStorageContentRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new EditStorageContentCommand(request.EditedFields);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Редактирование позиций на складе, количества, цены итд")
            .WithDisplayName("Редактирование позиций склада")
            .RequireAnyPermission("STORAGES.CONTENT.EDIT");
    }
}