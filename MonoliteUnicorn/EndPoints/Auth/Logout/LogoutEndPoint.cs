using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.Logout;

public record LogoutRequest(string Token);
public class LogoutEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout/", async (ISender sender, LogoutRequest request, string userId) =>
        {
            var command = new LogoutCommand(request.Token, userId);
            await sender.Send(command);
            return Results.Ok();
        }).WithGroup("Auth");
    }
}