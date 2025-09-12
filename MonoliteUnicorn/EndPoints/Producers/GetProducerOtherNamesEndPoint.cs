using Application.Handlers.Producers.GetProducerOtherNames;
using Carter;
using Core.Dtos.Amw.Producers;
using Core.Models;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Producers;

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
            .RequireAuthorization("AMW")
            .WithDisplayName("Получение дополнительных имен производителя")
            .WithDescription("Дополнительные имена производителя");
    }
}