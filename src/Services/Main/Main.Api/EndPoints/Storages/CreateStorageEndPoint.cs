using Main.Application.Handlers.Storages.CreateStorage;
using Carter;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record CreateStorageRequest(string Name, string? Description, string? Location);

public class CreateStorageEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/storages/",
                async (ISender sender, CreateStorageRequest request, CancellationToken cancellationToken) =>
                {
                    var command = request.Adapt<CreateStorageCommand>();
                    await sender.Send(command, cancellationToken);
                    return Results.Created();
                }).WithTags("Storages")
            .RequireAuthorization("AM")
            .WithDescription("Создание нового склада")
            .WithDisplayName("Создать склад");
    }
}