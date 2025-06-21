using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Storage;

namespace MonoliteUnicorn.EndPoints.Storages.EditStorageContent;

public record EditStorageContentRequest(Dictionary<int, (PatchStorageContentDto value, string concurrencyCode)> EditedFields);

public class EditStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/content", async (ISender sender, EditStorageContentRequest request, ClaimsPrincipal claims, CancellationToken cancellationToken) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
                return Results.Unauthorized();
            var command = new EditStorageContentCommand(request.EditedFields, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Storages")
        .WithDescription("Редактирование позиций на складе, количества, цены итд")
        .WithDisplayName("Редактирование позиций склада");
    }
}