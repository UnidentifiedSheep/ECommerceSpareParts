using Carter;
using Core.Models;
using Main.Application.Handlers.Producers.GetProducerOtherNames;
using Main.Core.Dtos.Amw.Producers;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Producers;

public record GetProducerOtherNamesResponse(IEnumerable<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers/{producerId}/names",
                async (ISender sender, int producerId, int page, int viewCount, CancellationToken token) =>
                {
                    var query = new GetProducerOtherNamesQuery(producerId, new PaginationModel(page, viewCount));
                    var result = await sender.Send(query, token);
                    return Results.Ok(result.Adapt<GetProducerOtherNamesResponse>());
                }).WithTags("Producers")
                .WithDisplayName("Получение дополнительных имен производителя")
                .WithDescription("Дополнительные имена производителя");
    }
}