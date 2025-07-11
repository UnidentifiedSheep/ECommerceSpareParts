﻿using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Auth.LoginAccount
{
	public record LoginRequest(string Email, string Password);
	public record LoginResponse(string Token, string RefreshToken);
	public class LoginEndPoint : ICarterModule
	{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/login", async (LoginRequest request, ISender sender) =>
			{
				var command = request.Adapt<LoginCommand>();
				var result = await sender.Send(command);
				var response = result.Adapt<LoginResponse>();

				return Results.Ok(response);
			}).WithName("Login")
			  .Produces<LoginResponse>(StatusCodes.Status200OK)
			  .ProducesProblem(StatusCodes.Status400BadRequest)
			  .ProducesProblem(StatusCodes.Status401Unauthorized)
			  .WithSummary("Login User")
			  .WithDescription("Login User")
			  .WithGroup("Auth");
		}
	}
}
