using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Dtos.Amw.Storage;

namespace MonoliteUnicorn.EndPoints.Storages.GetStorageContent;

public record GetStorageContentRequest(
    [FromQuery(Name = "storageName")] string? StorageName,
    [FromQuery(Name = "articleId")] int? ArticleId,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "viewCount")] int ViewCount,
    [FromQuery(Name = "showZeroContent")] bool ShowZeroCount = true);
public record GetStorageContentResponse(IEnumerable<StorageContentDto> Content);

public class GetStorageContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/content", async (ISender sender, CancellationToken token, [AsParameters] GetStorageContentRequest request) =>
        {
            var query = request.Adapt<GetStorageContentQuery>();
            var result = await sender.Send(query, token);
            var response = result.Adapt<GetStorageContentResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AMW")
        .WithGroup("Storages")
        .WithDescription("Получение списка позиций на складе по айди артикула или по названию склада")
        .WithDisplayName("Получение позиций склада");
    }
}