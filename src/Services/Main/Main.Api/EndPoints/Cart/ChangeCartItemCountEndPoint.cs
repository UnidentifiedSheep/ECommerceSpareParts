using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Cart.ChangeCartItemCount;
using Main.Enums;
using MediatR;
using Security.Extensions;

namespace Main.Api.EndPoints.Cart;

public record ChangeCartItemCountRequest(int NewCount);

public class ChangeCartItemCountEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/cart/{articleId}", async (ISender sender, ClaimsPrincipal user, int articleId,  
            ChangeCartItemCountRequest request, CancellationToken cancellationToken) =>
        {
            if (!user.GetUserId(out var userId)) return Results.Unauthorized();
            
            var command = new ChangeCartItemCountCommand(userId, articleId, request.NewCount);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithTags("Cart")
        .WithDescription("Редактирование позиции в корзине")
        .WithDisplayName("Редактирование позиции в корзине")
        .Produces(204)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(400)
        .RequireAnyPermission(PermissionCodes.CART_UPDATE);
    }
}