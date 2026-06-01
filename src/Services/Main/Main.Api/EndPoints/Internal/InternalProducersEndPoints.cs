using System.Text.Json.Serialization;
using Carter;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers.GetFullProducer;
using MediatR;

namespace Main.Api.EndPoints.Internal;

public record InternalGetFullProducerResponse
{
    [JsonPropertyName("producer")]
    public required ProducerDto Producer { get; init; }
    
    [JsonPropertyName("otherNames")]
    public required IReadOnlyList<ProducerOtherNameDto> OtherNames { get; init; }
}

public class InternalProducersEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/internal/producers")
            .WithGroupName("Internal Producers")
            .WithTags("InternalProducers");

        group.MapGet("{id:int}", async (
            int id, 
            ISender sender, 
            CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetFullProducerQuery(id), cancellationToken);
                return Results.Ok(new InternalGetFullProducerResponse
                {
                    Producer = result.Producer,
                    OtherNames = result.OtherNames
                });
            })
        .WithDisplayName("Internal service full producer")
        .WithName("InternalFullProducer")
        .WithSummary("Получить полного производителя для внутреннего сервиса")
        .WithDescription("Получение полного производителя, с его доп именами для внутренних интеграций")
        .Produces<InternalGetFullProducerResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
