using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Users.CreateMailForUser;

public record CreateMailForUserRequest(string MailBox, string? Password, string? Comment);
public record CreateMailForUserResponse(string MailBoxAddress, string Password);

public class CreateMailForUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/mail/corporate", async (ISender sender, string userId, CreateMailForUserRequest request, CancellationToken token) =>
            {
                var command = new CreateMailForUserCommand(userId, request.MailBox, request.Password, request.Comment);
                var result = await sender.Send(command, token);
                var response = result.Adapt<CreateMailForUserResponse>();
                string? uri = null;
                return Results.Created(uri, response);
            }).RequireAuthorization("AM")
        .WithGroup("Users")
        .WithDescription("Создание корпоративной почты для определенного пользователя")
        .WithDisplayName("Создание почты для пользователя");
    }
}