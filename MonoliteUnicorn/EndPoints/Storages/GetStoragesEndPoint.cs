using Application.Handlers.Storages.GetStorage;
using Carter;
using Core.Dtos.Amw.Storage;
using Core.Models;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Storages;

public record GetStoragesResponse(IEnumerable<StorageDto> Storages);

public class GetStoragesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/",
                async (ISender sender, int page, int viewCount, string? searchTerm, CancellationToken token) =>
                {
                    var query = new GetStoragesQuery(new PaginationModel(page, viewCount), searchTerm);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetStoragesResponse>();
                    return Results.Ok(response);
                }).RequireAuthorization("AMW")
            .WithTags("Storages")
            .WithDescription("Поиск и получение существующих складов")
            .WithDisplayName("Получение складов");
    }
}