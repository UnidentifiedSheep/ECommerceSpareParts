using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Search.Application.Dtos.Producers;
using Search.Application.Handlers.Producers.SearchProducers;

namespace Search.Api.EndPoints.Producers;

public record SearchProducersRequest : PaginationQueryModel
{
    [FromQuery(Name = "query")]
    public string? Query { get; init; }
}

public record SearchProducersResult
{
    [JsonPropertyName("producers")]
    public required IEnumerable<ProducerSearchDto> Producers { get; init; }
}

public static class SearchProducersEndPoint
{
    public static RouteGroupBuilder SearchProducers(this RouteGroupBuilder producers)
    {
        producers.MapGet("/", async (
                ISender sender,
                [AsParameters] SearchProducersRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SearchProducersQuery(
                        request.Query,
                        request),
                    cancellationToken);

                return Results.Ok(new SearchProducersResult
                {
                    Producers = result.Producers
                });
            })
            .WithTags("Producers")
            .RequireAllPermissions(PermissionCodes.ARTICLES_GET_MAIN)
            .WithDisplayName("Search producers")
            .WithSummary("Поиск производителей")
            .WithDescription("Ищет производителей по основному названию и дополнительным названиям, возвращает только данные производителя")
            .Produces<SearchProducersResult>();

        return producers;
    }
}
