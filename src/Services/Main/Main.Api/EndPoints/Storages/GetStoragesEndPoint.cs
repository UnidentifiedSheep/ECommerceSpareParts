using Carter;
using Core.Models;
using Main.Application.Handlers.Storages.GetStorage;
using Main.Core.Dtos.Amw.Storage;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Storages;

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
                }).WithTags("Storages")
            .WithDescription("Поиск и получение существующих складов")
            .WithDisplayName("Получение складов");
    }
}