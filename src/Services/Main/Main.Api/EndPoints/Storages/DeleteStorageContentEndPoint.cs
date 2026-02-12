using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.StorageContents.DeleteContent;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public class DeleteStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/content/{contentId}", async (ISender sender, int contentId, string concurrencyCode,
                IUserContext user, CancellationToken cancellationToken) =>
            {
                var command = new DeleteStorageContentCommand(contentId, concurrencyCode, user.UserId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Полное удаление позиции со склада по его Id")
            .WithDisplayName("Удаление позиции со склада")
            .RequireAnyPermission("STORAGES.CONTENT.DELETE");
    }
}