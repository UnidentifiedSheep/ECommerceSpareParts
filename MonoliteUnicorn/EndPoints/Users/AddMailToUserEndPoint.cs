using Application.Handlers.Users.AddMailToUser;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Users;

public record AddMailToUserRequest(string Email, bool IsVerified);

public class AddMailToUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/mail/external",
                async (ISender sender, string userId, AddMailToUserRequest request, CancellationToken token) =>
                {
                    var command = new AddMailToUserCommand(userId, request.Email, request.IsVerified);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).RequireAuthorization("AM")
            .WithTags("Users")
            .WithDescription("Добавление почты пользователю")
            .WithDisplayName("Добавление почты");
    }
}