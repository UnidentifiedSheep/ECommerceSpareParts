using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.GetPurchaseContent;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public record GetPurchaseContentResponse(IEnumerable<PurchaseContentDto> Content);

public class GetPurchaseContentEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/purchases/{id}/contents", async (ISender sender, string id, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPurchaseContentQuery(id), ct);
                var response = result.Adapt<GetPurchaseContentResponse>();
                return Results.Ok(response);
            }).WithTags("Purchases")
            .WithDescription("Получение содержания закупки")
            .WithDisplayName("Получение содержания закупки")
            .RequireAnyPermission("PURCHASE.GET");
    }
}