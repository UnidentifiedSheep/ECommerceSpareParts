using System.Security.Claims;
using Carter;
using Core.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public record CreatePurchaseRequest(
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum);

public class CreatePurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/purchases/",
                async (ClaimsPrincipal claims, ISender sender, CreatePurchaseRequest request,
                    CancellationToken token) =>
                {
                    if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                        return Results.Unauthorized();
                    var command = new CreateFullPurchaseCommand(userId, request.SupplierId, request.CurrencyId,
                        request.StorageName, request.PurchaseDate, request.PurchaseContent, request.Comment,
                        request.PayedSum);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).RequireAuthorization("AMW")
            .WithTags("Purchases")
            .WithDescription("Создание новой закупку")
            .WithDisplayName("Добавление артикулов");
    }
}