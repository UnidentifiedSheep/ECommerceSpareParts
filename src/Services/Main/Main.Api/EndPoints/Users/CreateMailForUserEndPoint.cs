using Carter;
using Main.Application.Handlers.Users;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record CreateMailForUserRequest(string MailBox, string? Password, string? Comment);

public record CreateMailForUserResponse(string MailBoxAddress, string Password);

public class CreateMailForUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId}/mail/corporate",
                async (ISender sender, string userId, CreateMailForUserRequest request, CancellationToken token) =>
                {
                    var command =
                        new CreateMailForUserCommand(userId, request.MailBox, request.Password, request.Comment);
                    var result = await sender.Send(command, token);
                    var response = result.Adapt<CreateMailForUserResponse>();
                    string? uri = null;
                    return Results.Created(uri, response);
                }).RequireAuthorization("AM")
            .WithTags("Users")
            .WithDescription("Создание корпоративной почты для определенного пользователя")
            .WithDisplayName("Создание почты для пользователя");
    }
}