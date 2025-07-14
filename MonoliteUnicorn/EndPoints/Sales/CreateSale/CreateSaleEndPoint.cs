using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Sales;

namespace MonoliteUnicorn.EndPoints.Sales.CreateSale;

public record CreateSaleRequest(
    string BuyerId,
    int CurrencyId,
    string StorageName,
    bool SellFromOtherStorages,
    DateTime SaleDateTime,
    IEnumerable<NewSaleContentDto> SaleContent,
    string? Comment,
    decimal? PayedSum); 

public class CreateSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/amw/sales/",
            async (HttpContext context, ISender sender, CreateSaleRequest request, CancellationToken token) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();
                var command = new CreateSaleCommand(userId, request.BuyerId, request.CurrencyId, 
                    request.StorageName, request.SellFromOtherStorages, 
                    request.SaleDateTime, request.SaleContent, request.Comment, request.PayedSum);
                await sender.Send(command, token);
                return Results.Ok();
            }).RequireAuthorization("AMW")
            .WithGroup("Sales")
            .WithDescription("Создание новой продажи")
            .WithDisplayName("Создание новой продажи");
    }
}