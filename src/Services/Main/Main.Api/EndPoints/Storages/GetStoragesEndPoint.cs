using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Exceptions.Exceptions.Storages;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.Storages.GetStorage;
using Main.Application.Handlers.Storages.GetStorageByName;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record GetStoragesResponse(IEnumerable<StorageDto> Storages);
public record GetStorageByNameResponse(StorageDto Storage);

public class GetStoragesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/",
                async (ISender sender, int page, int limit, string? searchTerm, StorageType? type, CancellationToken token) =>
                {
                    var query = new GetStoragesQuery(new PaginationModel(page, limit), searchTerm, type);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetStoragesResponse>();
                    return Results.Ok(response);
                }).WithTags("Storages")
            .WithDescription("Поиск и получение существующих складов")
            .Produces<GetStoragesResponse>()
            .WithDisplayName("Получение складов")
            .RequireAnyPermission("STORAGES.GET");
        
        app.MapGet("/storages/{name}",
                async (ISender sender, string name, CancellationToken token) =>
                {
                    var query = new GetStorageByNameQuery(name);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetStorageByNameResponse>();
                    return Results.Ok(response);
                }).WithTags("Storages")
            .WithDescription("Получение склада по имени")
            .WithDisplayName("Получение склада по имени")
            .Produces<GetStorageByNameResponse>()
            .Produces<StorageNotFoundException>(404)
            .RequireAnyPermission("STORAGES.GET");
    }
}