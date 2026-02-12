using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Sales.DeleteFullSale;
using MediatR;

namespace Main.Api.EndPoints.Sales;

public class DeleteSaleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/sales/{saleId}",
                async (ISender sender, IUserContext user, string saleId, CancellationToken token) =>
                {
                    var command = new DeleteFullSaleCommand(saleId, user.UserId);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Sales")
                .WithDescription("Удаление продажи")
                .WithDisplayName("Удаление продажи")
                .RequireAnyPermission("SALES.DELETE");
    }
}