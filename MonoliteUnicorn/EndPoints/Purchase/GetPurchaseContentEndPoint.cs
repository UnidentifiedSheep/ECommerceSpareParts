using Application.Handlers.Purchases.GetPurchaseContent;
using Carter;
using Core.Dtos.Amw.Purchase;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Purchase;

public record GetPurchaseContentResponse(IEnumerable<PurchaseContentDto> Content);

public class GetPurchaseContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/purchases/{id}/content", async (ISender sender, string id, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
            var response = result.Adapt<GetPurchaseContentResponse>();
            return Results.Ok(response);
        }).RequireAuthorization("AMW")
        .WithTags("Purchases")
        .WithDescription("Получение содержания закупки")
        .WithDisplayName("Получение содержания закупки");
    }
}