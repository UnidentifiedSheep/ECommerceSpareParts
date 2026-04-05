using Abstractions.Interfaces;
using Carter;
using Main.Application.Handlers.Auth.ChangePassword;
using MediatR;

namespace Main.Api.EndPoints.Auth;

public record ChangePasswordRequest(
    string PreviousPassword,
    string NewPassword);

public class ChangePasswordEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/password/", async (
            ISender sender,
            ChangePasswordRequest request,
            IUserContext user,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangePasswordCommand(user.UserId, request.PreviousPassword, request.NewPassword);
            await sender.Send(command, cancellationToken);
            
            return Results.Ok();
        }).WithName("ChangePassword")
        .WithDisplayName("Change Password");
    }
}