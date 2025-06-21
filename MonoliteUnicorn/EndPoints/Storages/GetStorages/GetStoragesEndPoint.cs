using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Storage;

namespace MonoliteUnicorn.EndPoints.Storages.GetStorages;

public record GetStoragesResponse(IEnumerable<StorageDto> Storages);

public class GetStoragesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/", async (ISender sender, int page, int viewCount, string? searchTerm, CancellationToken token) =>
        {
            var query = new GetStoragesQuery(page, viewCount, searchTerm);
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetStoragesResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AMW")
        .WithGroup("Storages")
        .WithDescription("Поиск и получение существующих складов")
        .WithDisplayName("Получение складов");
    }
}