using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Producers;

namespace MonoliteUnicorn.EndPoints.Producers.GetProducerOtherNames;

public record GetProducerOtherNamesResponse(IEnumerable<ProducerOtherNameDto> Names);

public class GetProducerOtherNamesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/producers/{producerId}/names",
            async (ISender sender, int producerId, int page, int viewCount, CancellationToken token) =>
            {
                var query = new GetProducerOtherNamesQuery(producerId, page, viewCount);
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetProducerOtherNamesResponse>());
            }).WithGroup("Producers")
            .RequireAuthorization("AMW")
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя");
    }
}