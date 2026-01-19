using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Main.Application.Seeding;

public static class UserSeed
{
    public static async Task SeedAdmin(string login, string password, string email, IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        CreateUserCommand command = new CreateUserCommand(login, password, new UserInfoDto
        {
            Name = "Admin",
            Surname = "Admin"
        }, [ new EmailDto
        {
            Email = email,
            IsConfirmed = true,
            IsPrimary = true,
            Type = EmailType.Personal
        }], [], ["ADMIN"]);
        await mediator.Send(command);
    }
}