using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Application.Handlers.Sales.CreateFullSale;
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
                async (IUserContext user, ISender sender, CreateSaleRequest request, CancellationToken token) =>
                {
                    var command = new CreateFullSaleCommand(user.UserId, request.BuyerId, request.CurrencyId,
                        request.StorageName, request.SellFromOtherStorages,
                        request.SaleDateTime, request.SaleContent, request.Comment, request.PayedSum,
                        request.ConfirmationCode);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Sales")
                .WithDescription("Создание новой продажи")
                .WithDisplayName("Создание новой продажи")
                .Produces(200)
                .Produces(401)
                .RequireAnyPermission("SALES.CREATE");
    }
}