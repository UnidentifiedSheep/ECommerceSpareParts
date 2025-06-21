using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.CreatePurchase;

public record CreatePurchaseRequest(
    string SupplierId,
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
        app.MapPost("/purchases/", async (HttpContext context, ISender sender, CreatePurchaseRequest request, CancellationToken token) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var command = new CreatePurchaseCommand(userId, request.SupplierId, request.CurrencyId, 
                    request.StorageName, request.PurchaseDate, request.PurchaseContent, 
                    request.Comment, request.PayedSum);
                await sender.Send(command, token);
                return Results.Ok();
            }).RequireAuthorization("AMW")
        .WithGroup("Purchases")
        .WithDescription("Создание новой закупку")
        .WithDisplayName("Добавление артикулов");
    }
}