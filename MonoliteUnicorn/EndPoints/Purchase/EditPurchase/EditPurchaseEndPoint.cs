using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Purchase;

namespace MonoliteUnicorn.EndPoints.Purchase.EditPurchase;

public record EditPurchaseRequest(
    IEnumerable<EditPurchaseDto> Content,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime);

public class EditPurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/purchase/{purchaseId}", async (ISender sender, string purchaseId, EditPurchaseRequest request, 
            CancellationToken cancellationToken, ClaimsPrincipal claims) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var command = new EditPurchaseCommand(request.Content, purchaseId, request.CurrencyId, 
                request.Comment, request.PurchaseDateTime, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithGroup("Purchases")
        .WithDescription("Редактирование существующей закупки")
        .WithDisplayName("Редактирование закупки");
    }
}