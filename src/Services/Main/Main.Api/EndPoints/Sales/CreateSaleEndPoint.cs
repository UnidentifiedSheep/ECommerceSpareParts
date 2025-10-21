using System.Security.Claims;
using Carter;
using Main.Application.Handlers.Sales.CreateFullSale;
using Main.Core.Dtos.Amw.Sales;
using MediatR;

namespace Main.Api.EndPoints.Sales;

public record CreateSaleRequest(
    Guid BuyerId,
    int CurrencyId,
    string StorageName,
    bool SellFromOtherStorages,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> SaleContent,
    string? Comment,
    decimal? PayedSum,
    string? ConfirmationCode);

public class CreateSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/sales/",
                async (ClaimsPrincipal claims, ISender sender, CreateSaleRequest request, CancellationToken token) =>
                {
                    if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                        return Results.Unauthorized();
                    var command = new CreateFullSaleCommand(userId, request.BuyerId, request.CurrencyId,
                        request.StorageName, request.SellFromOtherStorages,
                        request.SaleDateTime, request.SaleContent, request.Comment, request.PayedSum,
                        request.ConfirmationCode);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Sales")
                .WithDescription("Создание новой продажи")
                .WithDisplayName("Создание новой продажи");
    }
}