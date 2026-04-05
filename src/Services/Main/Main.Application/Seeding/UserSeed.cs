using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Application.Handlers.Users.CreateUser;
using Main.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Main.Application.Seeding;

public static class UserSeed
{
    public static async Task SeedAdmin(string login, string password, string email, IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateUserCommand>>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new CreateUserCommand(login, password, new UserInfoDto
        {
            Name = "Admin",
            Surname = "Admin"
        }, [
            new EmailDto
            {
                Email = email,
                IsConfirmed = true,
                IsPrimary = true,
                Type = EmailType.Personal
            }
        ], [], ["ADMIN"]);
        
        try
        {
            logger.LogInformation("Trying to create admin user: {login}", login);
            await mediator.Send(command);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to create admin user");
        }
    }
}