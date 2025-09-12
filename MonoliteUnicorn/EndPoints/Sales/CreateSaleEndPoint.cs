using System.Security.Claims;
using Application.Handlers.Sales.CreateFullSale;
using Carter;
using Core.Dtos.Amw.Sales;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Sales;

public record CreateSaleRequest(string BuyerId, int CurrencyId, string StorageName, bool SellFromOtherStorages,
    DateTime SaleDateTime, IEnumerable<NewSaleContentDto> SaleContent, string? Comment, decimal? PayedSum, string? ConfirmationCode);

public class CreateSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/amw/sales/",
            async (HttpContext context, ISender sender, CreateSaleRequest request, CancellationToken token) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();
                var command = new CreateFullSaleCommand(userId, request.BuyerId, request.CurrencyId, 
                    request.StorageName, request.SellFromOtherStorages, 
                    request.SaleDateTime, request.SaleContent, request.Comment, request.PayedSum, request.ConfirmationCode);
                await sender.Send(command, token);
                return Results.Ok();
            }).RequireAuthorization("AMW")
            .WithTags("Sales")
            .WithDescription("Создание новой продажи")
            .WithDisplayName("Создание новой продажи");
    }
}