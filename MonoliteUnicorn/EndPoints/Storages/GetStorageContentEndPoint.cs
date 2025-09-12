using Application.Handlers.StorageContents.GetContents;
using Carter;
using Core.Dtos.Amw.Storage;
using Core.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MonoliteUnicorn.EndPoints.Storages;

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
        app.MapGet("/storages/content",
                async (ISender sender, CancellationToken token, [AsParameters] GetStorageContentRequest request) =>
                {
                    var query = new GetStorageContentQuery(request.StorageName, request.ArticleId,
                        new PaginationModel(request.Page, request.ViewCount),
                        request.ShowZeroCount);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetStorageContentResponse>();
                    return Results.Ok(response);
                }).RequireAuthorization("AMW")
            .WithTags("Storages")
            .WithDescription("Получение списка позиций на складе по айди артикула или по названию склада")
            .WithDisplayName("Получение позиций склада");
    }
}