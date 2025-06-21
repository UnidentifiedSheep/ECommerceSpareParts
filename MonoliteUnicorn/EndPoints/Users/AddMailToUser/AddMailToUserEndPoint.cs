using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Users.AddMailToUser;

public record AddMailToUserRequest(string UserId, string Email, string? Comment, bool IsVerified); 

public class AddMailToUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/mail/external", async (ISender sender, AddMailToUserRequest request, CancellationToken token) =>
            {
                var command = request.Adapt<AddMailToUserCommand>();
                await sender.Send(command, token);
                return Results.NoContent();
            }).RequireAuthorization("AM")
        .WithGroup("Users")
        .WithDescription("Добавление почты пользователю")
        .WithDisplayName("Добавление почты");
    }
}