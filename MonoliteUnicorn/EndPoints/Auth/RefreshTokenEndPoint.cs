using Application.Handlers.Auth.RefreshToken;
using Carter;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth;

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(string Token, string RefreshToken);

public class RefreshTokenEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (ISender sender, RefreshTokenRequest request) =>
            {
                var command = request.Adapt<RefreshTokenCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<RefreshTokenResponse>();

                return Results.Ok(response);
            }).WithName("RefreshToken")
            .WithTags("Auth")
            .Produces<RefreshTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Refresh Token")
            .WithDescription("Refresh Token");
    }
}