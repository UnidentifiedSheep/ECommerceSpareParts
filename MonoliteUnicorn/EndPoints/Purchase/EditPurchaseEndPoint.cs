using System.Security.Claims;
using Application.Handlers.Purchases.EditFullPurchase;
using Carter;
using Core.Dtos.Amw.Purchase;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Purchase;

public record EditPurchaseRequest(IEnumerable<EditPurchaseDto> Content, int CurrencyId, string? Comment, DateTime PurchaseDateTime);

public class EditPurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/purchase/{purchaseId}", async (ISender sender, string purchaseId, EditPurchaseRequest request, 
            CancellationToken cancellationToken, ClaimsPrincipal claims) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var command = new EditFullPurchaseCommand(request.Content, purchaseId, request.CurrencyId, 
                request.Comment, request.PurchaseDateTime, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization("AMW")
        .WithTags("Purchases")
        .WithDescription("Редактирование существующей закупки")
        .WithDisplayName("Редактирование закупки");
    }
}