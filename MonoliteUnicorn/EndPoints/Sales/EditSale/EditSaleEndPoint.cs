using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Sales;

namespace MonoliteUnicorn.EndPoints.Sales.EditSale;

public record EditSaleRequest(IEnumerable<EditSaleContentDto> EditedContent, int CurrencyId, DateTime SaleDateTime, string? Comment, bool SellFromOtherStorages);
public class EditSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/sales/{saleId}", async (ISender sender, string saleId, EditSaleRequest request,
                ClaimsPrincipal claims, CancellationToken cancellationToken) =>
            {
                var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();
                var command = new EditSaleCommand(request.EditedContent, saleId, request.CurrencyId, userId, 
                    request.SaleDateTime, request.Comment, request.SellFromOtherStorages);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            }).RequireAuthorization("AMW")
            .WithGroup("Sales")
            .WithDescription("Редактирование продажи")
            .WithDisplayName("Редактирование продажи");
    }
}