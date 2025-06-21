using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.ConfirmMail;

public class ConfirmMailEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/verify/mail", async (ISender sender, string userId, string confirmationToken) =>
        {
            var query = new ConfirmMailQuery(userId, confirmationToken);
            var result = await sender.Send(query);
            return Results.Ok();
        }).WithGroup("Auth");
    }
}