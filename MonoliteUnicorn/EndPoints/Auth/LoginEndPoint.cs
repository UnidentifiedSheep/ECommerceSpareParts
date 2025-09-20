using Application.Handlers.Auth.Login;
using Carter;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, string RefreshToken);

public class LoginEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (LoginRequest request, ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
                var ipAddress = context.Connection.RemoteIpAddress;
                
                var command = new LoginCommand(request.Email, request.Password, ipAddress, userAgent);
                var result = await sender.Send(command, cancellationToken);
                var response = result.Adapt<LoginResponse>();

                return Results.Ok(response);
            }).WithName("Login")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Login User")
            .WithDescription("Login User")
            .WithTags("Auth");
    }
}