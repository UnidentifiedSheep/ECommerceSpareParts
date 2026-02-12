using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record CreateStorageRequest(string Name, string? Description, string? Location, StorageType Type);
public record CreateStorageResponse(string Name);

public class CreateStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storages/",
                async (ISender sender, CreateStorageRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.Adapt<CreateStorageCommand>();
                    var result = await sender.Send(command, cancellationToken);
                    var response = new CreateStorageResponse(result.Name);
                    return Results.Created($"/storages/", response);
                }).WithTags("Storages")
            .WithDescription("Создание нового склада")
            .WithDisplayName("Создать склад")
            .Produces<CreateStorageResponse>(201)
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.STORAGES_CREATE);
    }
}