using System.Security.Claims;
using Application.Handlers.Purchases.CreateFullPurchase;
using Carter;
using Core.Dtos.Amw.Purchase;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Purchase;

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
        app.MapPost("/purchases/",
                async (HttpContext context, ISender sender, CreatePurchaseRequest request, CancellationToken token) =>
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
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