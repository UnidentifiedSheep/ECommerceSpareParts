using Application.Handlers.Auth.ConfirmMail;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth;

public class ConfirmMailEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/verify/mail", async (ISender sender, string userId, string confirmationToken) =>
        {
            var query = new ConfirmMailCommand(userId, confirmationToken);
            var result = await sender.Send(query);
            return Results.Ok();
        }).WithTags("Auth");
    }
}