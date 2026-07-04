using Abstractions.Interfaces;
using Carter;
using Main.Application.Handlers.Auth;
using Main.Application.Handlers.Auth.ChangePassword;
using Main.Application.Handlers.Auth.PasswordRecovery.ResetPassword;
using Main.Application.Handlers.Auth.PasswordRecovery.SendEmailRecovery;
using Main.Application.Handlers.Auth.Register;
using MediatR;

namespace Main.Api.EndPoints;

public record ChangePasswordRequest(string PreviousPassword, string NewPassword);

public record SendEmailRecoveryRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string RefreshToken,
    string DeviceId
);

public record RefreshTokenRequest(string RefreshToken, string DeviceId);

public record RefreshTokenResponse(string Token, string RefreshToken);

public record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string Name,
    string Surname
);

public class AuthEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost(
                "/password/",
                async (
                    ISender sender,
                    ChangePasswordRequest request,
                    IUserContext user,
                    CancellationToken cancellationToken) =>
                {
                    var command = new ChangePasswordCommand(
                        user.UserId,
                        request.PreviousPassword,
                        request.NewPassword);
                    await sender.Send(command, cancellationToken);
                    return Results.Ok();
                })
            .WithName("ChangePassword")
            .WithSummary("Сменить пароль")
            .WithDisplayName("Change Password")
            .Accepts<ChangePasswordRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        auth.MapPost(
                "/password/recovery",
                async (
                    ISender sender,
                    SendEmailRecoveryRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new SendEmailRecoveryCommand(request.Email),
                        cancellationToken);

                    return Results.Ok();
                })
            .WithName("SendPasswordRecoveryEmail")
            .WithSummary("Send password recovery email")
            .WithDisplayName("Send Password Recovery Email")
            .Accepts<SendEmailRecoveryRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        auth.MapPost(
                "/password/reset",
                async (
                    ISender sender,
                    ResetPasswordRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new ResetPasswordCommand(request.Token, request.NewPassword),
                        cancellationToken);

                    return Results.Ok();
                })
            .WithName("ResetPassword")
            .WithSummary("Reset password")
            .WithDisplayName("Reset Password")
            .Accepts<ResetPasswordRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        auth.MapGet(
                "/verify/mail",
                async (
                    ISender sender,
                    string userId,
                    string confirmationToken) =>
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

        auth.MapPost(
                "/login",
                async (
                    LoginRequest request,
                    ISender sender,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                {
                    var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
                    var ipAddress = context.Connection.RemoteIpAddress;
                    var result = await sender.Send(
                        new LoginCommand(
                            request.Email,
                            request.Password,
                            ipAddress,
                            userAgent),
                        cancellationToken);
                    return Results.Ok(
                        new LoginResponse(
                            result.Token,
                            result.RefreshToken,
                            result.DeviceId));
                })
            .WithName("Login")
            .WithDisplayName("Вход пользователя")
            .Accepts<LoginRequest>(false, "application/json")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Login User")
            .WithDescription("Login User");

        auth.MapPost(
                "/refresh",
                async (
                    ISender sender,
                    RefreshTokenRequest request,
                    CancellationToken ct) =>
                {
                    var result = await sender.Send(
                        new RefreshTokenCommand(request.RefreshToken, request.DeviceId),
                        ct);
                    return Results.Ok(new RefreshTokenResponse(result.Token, result.RefreshToken));
                })
            .WithName("RefreshToken")
            .WithDisplayName("Обновление токена")
            .Accepts<RefreshTokenRequest>(false, "application/json")
            .Produces<RefreshTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Refresh Token")
            .WithDescription("Refresh Token");

        auth.MapPost(
                "/register/",
                async (
                    RegisterRequest request,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    await sender.Send(
                        new RegisterCommand(
                            request.Email,
                            request.UserName,
                            request.Password,
                            request.Name,
                            request.Surname),
                        ct);
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