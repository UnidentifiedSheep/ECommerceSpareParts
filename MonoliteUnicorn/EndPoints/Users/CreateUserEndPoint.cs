using Application.Handlers.Users.CreateUser;
using Carter;
using Core.Dtos.Amw.Users;
using Mapster;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Users;

public record CreateUserRequest(NewUserDto NewUser);
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
        .WithTags("Users")
        .WithDescription("Создание пользователя без пароля")
        .WithDisplayName("Создание пользователя");
    }
}