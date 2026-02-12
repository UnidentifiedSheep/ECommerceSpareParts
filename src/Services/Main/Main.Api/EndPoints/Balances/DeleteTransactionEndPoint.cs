using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Balance.DeleteTransaction;
using MediatR;

namespace Main.Api.EndPoints.Balances;

public class DeleteTransactionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/balances/transaction/{id}",
                async (ISender sender, IUserContext user, Guid id, CancellationToken token) =>
                {
                    var command = new DeleteTransactionCommand(id, user.UserId);
                    await sender.Send(command, token);
                    return Results.Ok();
                }).WithTags("Balances")
            .WithDescription("Удалить транзакцию")
            .WithDisplayName("Удалить транзакцию")
            .RequireAnyPermission("BALANCES.TRANSACTION.DELETE");
    }
}