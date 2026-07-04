using System.Text.Json.Serialization;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullProducersResponse
{
    [JsonPropertyName("producers")]
    public required IReadOnlyList<ProducerFullDto> Producers { get; init; }
}

public record InternalGetFullProducersRequest
{
    [FromQuery(Name = "id")]
    public int[] ProducerIds { get; init; } = [];
}

public static class InternalProducerEndPoints
{
    public static RouteGroupBuilder AddInternalProducerEndPoints(this RouteGroupBuilder group)
    {
        var producers = group
            .MapGroup("/producers")
            .WithGroupName("Internal Producers")
            .WithTags("InternalProducers");

        producers.MapGet(
                "",
                async (
                    [AsParameters] InternalGetFullProducersRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetFullProducersQuery(request.ProducerIds), cancellationToken);
                    return Results.Ok(
                        new InternalGetFullProducersResponse
                        {
                            Producers = result.Producers
                        });
                })
            .WithDisplayName("Internal service full producer")
            .WithName("InternalFullProducer")
            .WithSummary("Получить полного производителя для внутреннего сервиса")
            .WithDescription("Получение полного производителя, с его доп именами для внутренних интеграций")
            .Produces<InternalGetFullProducersResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}