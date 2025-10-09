using Carter;
using Main.Application.Handlers.Auth.ConfirmMail;
using MediatR;

namespace Main.Api.EndPoints.Auth;

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