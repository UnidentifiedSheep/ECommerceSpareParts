using Abstractions.Interfaces;
using Carter;
using Main.Application.Handlers.Auth.ChangePassword;
using Main.Application.Handlers.Auth.ConfirmMail;
using Main.Application.Handlers.Auth.Login;
using Main.Application.Handlers.Auth.RefreshToken;
using Main.Application.Handlers.Auth.Register;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Auth;

public record ChangePasswordRequest(string PreviousPassword, string NewPassword);

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, string RefreshToken, string DeviceId);

public record RefreshTokenRequest(string RefreshToken, string DeviceId);

public record RefreshTokenResponse(string Token, string RefreshToken);

public record RegisterRequest(string Email, string UserName, string Password, string Name, string Surname);

public class AuthEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost("/password/", async (
                ISender sender,
                ChangePasswordRequest request,
                IUserContext user,
                CancellationToken cancellationToken) =>
            {
                var command = new ChangePasswordCommand(user.UserId, request.PreviousPassword, request.NewPassword);
                await sender.Send(command, cancellationToken);
                return Results.Ok();
            })
            .WithName("ChangePassword")
            .WithSummary("Сменить пароль")
            .WithDisplayName("Change Password")
            .Accepts<ChangePasswordRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        auth.MapGet("/verify/mail", async (ISender sender, string userId, string confirmationToken) =>
            {
                await sender.Send(new ConfirmMailCommand(userId, confirmationToken));
                return Results.Ok();
            })
            .WithName("ConfirmMail")
            .WithSummary("Подтвердить почту")
            .WithDescription("Подтверждение почты пользователя")
            .WithDisplayName("Подтверждение почты")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        auth.MapPost("/login", async (
                LoginRequest request,
                ISender sender,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
                var ipAddress = context.Connection.RemoteIpAddress;
                var result = await sender.Send(
                    new LoginCommand(request.Email, request.Password, ipAddress, userAgent),
                    cancellationToken);
                return Results.Ok(result.Adapt<LoginResponse>());
            })
            .WithName("Login")
            .WithDisplayName("Вход пользователя")
            .Accepts<LoginRequest>(false, "application/json")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Login User")
            .WithDescription("Login User");

        auth.MapPost("/refresh", async (ISender sender, RefreshTokenRequest request) =>
            {
                var result = await sender.Send(request.Adapt<RefreshTokenCommand>());
                return Results.Ok(result.Adapt<RefreshTokenResponse>());
            })
            .WithName("RefreshToken")
            .WithDisplayName("Обновление токена")
            .Accepts<RefreshTokenRequest>(false, "application/json")
            .Produces<RefreshTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Refresh Token")
            .WithDescription("Refresh Token");

        auth.MapPost("/register/", async (RegisterRequest request, ISender sender) =>
            {
                await sender.Send(request.Adapt<RegisterCommand>());
                return Results.Ok();
            })
            .WithName("RegisterUser")
            .WithDisplayName("Регистрация пользователя")
            .Accepts<RegisterRequest>(false, "application/json")
            .Produces<Unit>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register User")
            .WithDescription("Register User");
    }
}
