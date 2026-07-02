using System.Text.Json.Serialization;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers.GetFullProducer;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullProducerResponse
{
    [JsonPropertyName("producer")]
    public required ProducerDto Producer { get; init; }

    [JsonPropertyName("aliases")]
    public required IReadOnlyList<ProducerAliasDto> Aliases { get; init; }
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
                "{id:int}",
                async (
                    int id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetFullProducerQuery(id), cancellationToken);
                    return Results.Ok(
                        new InternalGetFullProducerResponse
                        {
                            Producer = result.Producer,
                            Aliases = result.Aliases
                        });
                })
            .WithDisplayName("Internal service full producer")
            .WithName("InternalFullProducer")
            .WithSummary("Получить полного производителя для внутреннего сервиса")
            .WithDescription("Получение полного производителя, с его доп именами для внутренних интеграций")
            .Produces<InternalGetFullProducerResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}