using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Application.Handlers.Sales.EditFullSale;
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
                IUserContext user, CancellationToken cancellationToken) =>
            {
                var command = new EditFullSaleCommand(request.EditedContent, saleId, request.CurrencyId, user.UserId,
                    request.SaleDateTime, request.Comment, request.SellFromOtherStorages);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            }).WithTags("Sales")
            .WithDescription("Редактирование продажи")
            .WithDisplayName("Редактирование продажи")
            .RequireAnyPermission("SALES.EDIT");
    }
}