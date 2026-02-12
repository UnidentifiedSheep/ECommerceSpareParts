using Abstractions.Models;
using Carter;
using Main.Application.Handlers.Producers.GetProducerOtherNames;
using Main.Abstractions.Dtos.Amw.Producers;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record GetProducerOtherNamesResponse(IEnumerable<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers/{producerId}/names",
                async (ISender sender, int producerId, int page, int limit, CancellationToken token) =>
                {
                    var query = new GetProducerOtherNamesQuery(producerId, new PaginationModel(page, limit));
                    var result = await sender.Send(query, token);
                    return Results.Ok(result.Adapt<GetProducerOtherNamesResponse>());
                }).WithTags("Producers")
                .WithDisplayName("Получение дополнительных имен производителя")
                .WithDescription("Дополнительные имена производителя");
    }
}