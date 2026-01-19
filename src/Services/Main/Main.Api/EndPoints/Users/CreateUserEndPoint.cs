using Api.Common.Extensions;
using Carter;
using Exceptions.Exceptions.Users;
using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record CreateUserRequest(
    string UserName,
    string Password,
    UserInfoDto UserInfo,
    IEnumerable<EmailDto> Emails,
    IEnumerable<string> Phones,
    IEnumerable<string> Roles);

public class CreateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/", async (ISender sender, CreateUserRequest request, CancellationToken cancellationToken) =>
            {
                var command = request.Adapt<CreateUserCommand>();
                var userId = (await sender.Send(command, cancellationToken)).UserId;
                return Results.Created($"users/{userId}", null);
            }).WithTags("Users")
            .WithDescription("Создание пользователя")
            .WithDisplayName("Создание пользователя")
            .Produces<EmailAlreadyTakenException>(409)
            .RequireAnyPermission("USERS.CREATE");
    }
}