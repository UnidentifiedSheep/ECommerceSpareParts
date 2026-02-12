using System.Security.Claims;
using Abstractions.Interfaces;
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
                IUserContext user, CancellationToken cancellationToken) =>
            {
                var command = new AddContentCommand(request.StorageContent, request.StorageName, user.UserId,
                    StorageMovementType.StorageContentAddition);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Storages")
            .WithDescription("Добавление позиций на склад")
            .WithDisplayName("Добавление позиций на склад")
            .RequireAnyPermission("STORAGES.CONTENT.CREATE");
    }
}