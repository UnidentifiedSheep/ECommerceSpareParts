using Main.Application.Handlers.Users.CreateUser;
using Main.Core.Dtos.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application.Seeding;

public static class UserSeed
{
    public static async Task SeedAdmin(string login, string password, IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        CreateUserCommand command = new CreateUserCommand(login, password, new UserInfoDto
        {
            Name = "Admin",
            Surname = "Admin"
        }, [], [], ["ADMIN"]);
        await mediator.Send(command);
    }
}