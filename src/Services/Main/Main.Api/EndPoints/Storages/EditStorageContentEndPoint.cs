using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Models;
using Main.Application.Handlers.StorageContents.EditContent;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record EditStorageContentRequest(
    Dictionary<int, ModelWithCode<PatchStorageContentDto, string>> EditedFields);

public class EditStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/content", async (ISender sender, EditStorageContentRequest request,
                IUserContext user, CancellationToken cancellationToken) =>
            {
                var command = new EditStorageContentCommand(request.EditedFields, user.UserId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Редактирование позиций на складе, количества, цены итд")
            .WithDisplayName("Редактирование позиций склада")
            .RequireAnyPermission("STORAGES.CONTENT.EDIT");
    }
}