using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Purchase;

public record CreatePurchaseRequest(
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom);

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
                        request.PayedSum, request.WithLogistics, request.StorageFrom);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Purchases")
                .WithDescription("Создание новой закупку")
                .WithDisplayName("Создание новой закупку")
                .RequireAnyPermission(PermissionCodes.PURCHASE_CREATE);
    }
}