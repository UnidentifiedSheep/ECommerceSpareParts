using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Storage;

namespace MonoliteUnicorn.EndPoints.Storages.AddContentToStorage;

public record AddContentToStorageRequest(IEnumerable<NewStorageContentDto> StorageContent, string StorageName);
    
public class AddContentToStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storages/content", async (ISender sender, AddContentToStorageRequest request, ClaimsPrincipal claims, CancellationToken cancellationToken) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null) return Results.Unauthorized();
            var command = new AddContentToStorageCommand(request.StorageContent, request.StorageName, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Storages")
        .WithDescription("Добавление позиций на склад")
        .WithDisplayName("Добавление позиций на склад");
    }
}