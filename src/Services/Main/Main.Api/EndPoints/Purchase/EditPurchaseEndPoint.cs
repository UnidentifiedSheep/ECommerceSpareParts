using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.EditFullPurchase;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public record EditPurchaseRequest(
    IEnumerable<EditPurchaseDto> Content,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    bool WithLogistics,
    string? StorageFrom);

public class EditPurchaseEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/purchases/{purchaseId}", async (
                ISender sender,
                string purchaseId,
                EditPurchaseRequest request,
                CancellationToken cancellationToken,
                IUserContext user) =>
            {
                var command = new EditFullPurchaseCommand(request.Content, purchaseId, request.CurrencyId,
                    request.Comment, request.PurchaseDateTime, user.UserId, request.WithLogistics, request.StorageFrom);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Purchases")
            .WithDescription("Редактирование существующей закупки")
            .WithDisplayName("Редактирование закупки")
            .RequireAnyPermission("PURCHASE.EDIT");
    }
}