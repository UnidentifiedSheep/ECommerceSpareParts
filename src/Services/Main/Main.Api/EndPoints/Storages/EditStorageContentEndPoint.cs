using System.Security.Claims;
using Carter;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Models;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record EditStorageContentRequest(
    Dictionary<int, ModelWithCode<PatchStorageContentDto, string>> EditedFields);

public class EditStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/content", async (ISender sender, EditStorageContentRequest request,
                ClaimsPrincipal claims, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new EditStorageContentCommand(request.EditedFields, userId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Редактирование позиций на складе, количества, цены итд")
            .WithDisplayName("Редактирование позиций склада");
    }
}