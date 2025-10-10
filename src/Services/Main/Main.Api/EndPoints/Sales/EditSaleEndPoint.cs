using System.Security.Claims;
using Carter;
using Main.Application.Handlers.Sales.EditFullSale;
using Main.Core.Dtos.Amw.Sales;
using MediatR;

namespace Main.Api.EndPoints.Sales;

public record EditSaleRequest(
    IEnumerable<EditSaleContentDto> EditedContent,
    int CurrencyId,
    DateTime SaleDateTime,
    string? Comment,
    bool SellFromOtherStorages);

public class EditSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/sales/{saleId}", async (ISender sender, string saleId, EditSaleRequest request,
                ClaimsPrincipal claims, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new EditFullSaleCommand(request.EditedContent, saleId, request.CurrencyId, userId,
                    request.SaleDateTime, request.Comment, request.SellFromOtherStorages);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            }).RequireAuthorization("AMW")
            .WithTags("Sales")
            .WithDescription("Редактирование продажи")
            .WithDisplayName("Редактирование продажи");
    }
}