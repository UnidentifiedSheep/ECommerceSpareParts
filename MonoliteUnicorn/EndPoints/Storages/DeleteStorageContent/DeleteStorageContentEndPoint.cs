using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Storages.DeleteStorageContent;

public class DeleteStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/content/{contentId}", async (ISender sender, int contentId, string concurrencyCode, ClaimsPrincipal claims, CancellationToken cancellationToken) => 
            { 
                var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                if(userId == null)
                    return Results.Unauthorized();
                var command = new DeleteStorageContentCommand(contentId, concurrencyCode, userId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent(); 
            }).RequireAuthorization("AM")
            .WithGroup("Storages")
            .WithDescription("Полное удаление позиции со склада по его Id")
            .WithDisplayName("Удаление позиции со склада");
    }
}