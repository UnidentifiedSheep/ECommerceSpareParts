using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.RefreshToken
{
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
				.WithGroup("Auth")
				.Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.WithSummary("Refresh Token")
				.WithDescription("Refresh Token");
		}
	}
}
