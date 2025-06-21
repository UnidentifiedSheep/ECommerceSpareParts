using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.Users;

namespace MonoliteUnicorn.EndPoints.Users.CreateUser;

public record CreateUserRequest(AmwNewUserDto NewUser);
public record CreateUserResponse(string UserId);

public class CreateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/", async (ISender sender, CreateUserRequest request, CancellationToken token) =>
        {
            var command = request.Adapt<CreateUserCommand>();
            var result = await sender.Send(command, token);
            var response = result.Adapt<CreateUserResponse>();
            return Results.Created($"/users/{response.UserId}", response);
        }).RequireAuthorization("AMW")
        .WithGroup("Users")
        .WithDescription("Создание пользователя без пароля")
        .WithDisplayName("Создание пользователя");
    }
}