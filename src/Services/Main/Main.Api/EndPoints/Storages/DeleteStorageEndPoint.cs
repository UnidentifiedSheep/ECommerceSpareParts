using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Storages.DeleteStorage;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public class DeleteStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/{storageName}",
                async (ISender sender, string storageName, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteStorageCommand(storageName);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                }).WithTags("Storages")
            .WithDescription("Полное удаление склада по его имени")
            .WithDisplayName("Удаление склада")
            .RequireAnyPermission("STORAGES.DELETE");
    }
}