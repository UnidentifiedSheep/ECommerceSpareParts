using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record AddContentToStorageRequest(IEnumerable<NewStorageContentDto> StorageContent, string StorageName);

public class AddContentToStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storages/content", async (ISender sender, AddContentToStorageRequest request,
                ClaimsPrincipal claims, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new AddContentCommand(request.StorageContent, request.StorageName, userId,
                    StorageMovementType.StorageContentAddition);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Добавление позиций на склад")
            .WithDisplayName("Добавление позиций на склад")
            .RequireAnyPermission("STORAGES.CONTENT.CREATE");
    }
}