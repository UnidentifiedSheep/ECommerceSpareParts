using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Application.Handlers.StorageContents.GetContents;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Storages;

public record GetStorageContentRequest(
    [FromQuery(Name = "storageName")] string? StorageName,
    [FromQuery(Name = "articleId")] int? ArticleId,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit,
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
                        new PaginationModel(request.Page, request.Limit),
                        request.ShowZeroCount);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetStorageContentResponse>();
                    return Results.Ok(response);
                }).WithTags("Storages")
            .WithDescription("Получение списка позиций на складе по айди артикула или по названию склада")
            .WithDisplayName("Получение позиций склада")
            .RequireAnyPermission("STORAGES.CONTENT.GET.ALL");
    }
}